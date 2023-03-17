using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Extensions;
using poshtar.Services;
using Serilog;
using Serilog.Events;

namespace poshtar;

public class Program
{
    public static string CertCrt { get; private set; } = string.Empty;
    public static string CertKey { get; private set; } = string.Empty;

    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(C.IsDebug ? LogEventLevel.Debug : LogEventLevel.Information)
                .MinimumLevel.Override(nameof(Microsoft), LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();
            builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
            builder.Services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();
            switch (C.DbContextType)
            {
                case DbContextType.PostgreSQL: builder.Services.AddDbContext<AppDbContext, PostgresDbContext>(); break;
                case DbContextType.MySQL: builder.Services.AddDbContext<AppDbContext, MysqlDbContext>(); break;
                case DbContextType.SQLite: builder.Services.AddDbContext<AppDbContext, SqliteDbContext>(); break;
            }

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

            builder.Services.AddControllers(options =>
                {
                    options.Filters.Add<ExceptionFilter>();
                })
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull;
                    o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    o.JsonSerializerOptions.WriteIndented = true;
                });
            // In production, the React files will be served from this directory
            builder.Services.AddSpaStaticFiles(c => { c.RootPath = "client-app"; });

            var app = builder.Build();
            await Initialize(app.Services);
            await StartServices();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
                app.UseForwardedHeaders();

            app.UseSpaStaticFiles();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers().RequireAuthorization();

            app.MapWhen(x => !x.Request.Path.Value!.StartsWith("/api/"), builder =>
            {
                builder.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "../client-app";
                    if (app.Environment.IsDevelopment())
                        spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
                });
            });

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
        Directory.CreateDirectory(C.Paths.MailData);
        Directory.CreateDirectory(C.Paths.LogData);

        if (string.IsNullOrWhiteSpace(C.Hostname))
            throw new Exception("You must specify HOSTNAME environment variable");

        if (!File.Exists(C.Paths.CertCrt) || !File.Exists(C.Paths.CertKey))
            throw new Exception($"Could not load certs from {C.Paths.CertData}");

        if (!Directory.Exists(DovecotConfiguration.DovecotRoot))
            DovecotConfiguration.Generate();

        if (!Directory.Exists(PostfixConfiguration.PostfixRoot))
            PostfixConfiguration.Generate();

        using var scope = provider.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (db.Database.GetMigrations().Any())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        // Demo data
        if (C.IsDebug && !db.Domains.Any())
        {
            var dpProvider = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>();
            await db.InitializeDefaults(dpProvider);
        }
    }
    static async Task StartServices()
    {
        if (C.StartApiOnly)
            return;

        var dovecot = await BashExec.StartDovecotAsync();
        if (dovecot.exitCode == 0)
            Log.Debug("Dovecot started");
        else
            Log.Error("Could not start dovecot: {error}", dovecot.error);

        var postfix = await BashExec.StartPostfixAsync();
        if (postfix.exitCode == 0)
            Log.Debug("Postfix started");
        else
            Log.Error("Could not start postfix: {error}", postfix.error);
    }
}

