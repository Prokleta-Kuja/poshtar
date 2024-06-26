using System.Text.Json;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using poshtar.Entities;
using poshtar.Extensions;
using poshtar.Services;
using poshtar.Smtp;
using Serilog;
using Serilog.Events;

namespace poshtar;

public class Program
{
    static readonly string[] nonSpaPrefixes = new string[] { "/.well-known/", "/dav", "/api/" };
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(C.IsDebug ? LogEventLevel.Debug : LogEventLevel.Information)
                .MinimumLevel.Override(nameof(Microsoft), LogEventLevel.Warning)
                .MinimumLevel.Override(nameof(Hangfire), LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();
            builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);
            builder.Services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();
            switch (C.DbContextType)
            {
                case DbContextType.PostgreSQL: builder.Services.AddDbContext<AppDbContext, PostgresDbContext>(); break;
                case DbContextType.MySQL: builder.Services.AddDbContext<AppDbContext, MysqlDbContext>(); break;
                case DbContextType.SQLite: builder.Services.AddDbContext<AppDbContext, SqliteDbContext>(); break;
            }

            builder.Services.AddMemoryCache();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.UseOneOfForPolymorphism();
                options.UseAllOfForInheritance();

                options.DescribeAllParametersInCamelCase();
                options.SchemaFilter<OpenApiEnumSchemaFilter>();
                options.SupportNonNullableReferenceTypes();
                options.UseAllOfToExtendReferenceSchemas();
            });

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opt =>
                {
                    opt.Cookie.Name = ".poshtar.auth";
                    opt.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
                    opt.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
                });

            builder.Services.AddControllers(opt =>
                {
                    opt.Filters.Add<ExceptionFilter>();
                })
                .ConfigureApiBehaviorOptions(opt =>
                {
                    opt.InvalidModelStateResponseFactory = BadRequestFactory.Handle;
                })
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull;
                    o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    o.JsonSerializerOptions.WriteIndented = true;
                });
            // In production, the React files will be served from this directory
            builder.Services.AddSpaStaticFiles(c => { c.RootPath = "web"; });

            // Add Hangfire services.
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage(C.Paths.Hangfire, new SQLiteStorageOptions
                {
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    InvisibilityTimeout = TimeSpan.FromMinutes(30),
                    DistributedLockLifetime = TimeSpan.FromSeconds(30),
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),
                    CountersAggregateInterval = TimeSpan.FromMinutes(5),
                }));

            // Remove Hangfire culture filter
            var captureFilter = GlobalJobFilters.Filters.OfType<JobFilter>().Where(c => c.Instance is CaptureCultureAttribute).FirstOrDefault();
            if (captureFilter != null)
                GlobalJobFilters.Filters.Remove(captureFilter.Instance);

            // Add the processing server as IHostedService
            builder.Services.AddHangfireServer(o =>
            {
                o.ServerName = nameof(poshtar);
                o.WorkerCount = Math.Max(2, Environment.ProcessorCount / 2);
            });

            builder.Services.AddHttpClient<HibpService>();
            builder.Services.AddSingleton<IpService>();

            var app = builder.Build();
            await Initialize(app.Services);
            await app.UseDovecotAsync();
            app.UseSmtp();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
                app.UseForwardedHeaders();

            app.UseSpaStaticFiles();
            if (C.IsDebug) // Reverse proxy will handle the redirection
                app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseJobDashboard();
            app.ReregisterRecurringJobs();

            app.MapControllers().RequireAuthorization();

            app.MapWhen(x => !nonSpaPrefixes.Any(prefix => x.Request.Path.Value!.StartsWith(prefix)), builder =>
            {
                builder.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "../web";
                    if (app.Environment.IsDevelopment())
                        spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
                });
            });

            Log.Information("App started");
            app.Run();

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    static async Task Initialize(IServiceProvider provider)
    {
        Directory.CreateDirectory(C.Paths.CertData);
        Directory.CreateDirectory(C.Paths.ConfigData);
        Directory.CreateDirectory(C.Paths.DovecotData);
        Directory.CreateDirectory(C.Paths.UserData);
        Directory.CreateDirectory(C.Paths.CalendarObjectsData);
        Directory.CreateDirectory(C.Paths.QueueData);

        if (string.IsNullOrWhiteSpace(C.Hostname))
            throw new Exception("You must specify HOSTNAME environment variable");

        if (!File.Exists(C.Paths.CertCrt) || !File.Exists(C.Paths.CertKey))
            throw new Exception($"Could not load certs from {C.Paths.CertData}");

        await C.Settings.LoadAsync();
        DovecotConfiguration.Generate();

        using var scope = provider.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (!C.IsDebug && db.Database.GetMigrations().Any())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        if (C.IsDebug && !db.Domains.Any())
        {
            var dpProvider = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>();
            await db.InitializeDefaults(dpProvider);
        }

        var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
        var blockedIps = await db.BlockedIps.ToListAsync();
        var banDuration = TimeSpan.FromHours(C.Smtp.AntiSpamSettings.BanHours);
        var now = DateTime.UtcNow;
        foreach (var blockedIp in blockedIps)
        {
            var elapsed = now - blockedIp.LastHit;
            if (elapsed < banDuration)
            {
                var key = AntiSpam.GetBannedIpKey(blockedIp.Address);
                cache.Set(key, blockedIp.Reason, banDuration - elapsed);
            }
        }
    }
}
