using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using poshtar.Endpoints;
using poshtar.Entities;
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
            builder.Services.AddSmtp();
            builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
            builder.Services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();
            builder.Services.AddDbContextFactory<AppDbContext>(builder =>
            {
                builder.UseSqlite(C.Paths.AppDbConnectionString);
                if (C.IsDebug)
                {
                    builder.EnableSensitiveDataLogging();
                    builder.LogTo(message => Debug.WriteLine(message), new[] { RelationalEventId.CommandExecuted });
                }
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.UseOneOfForPolymorphism();
                options.UseAllOfForInheritance();

                options.DescribeAllParametersInCamelCase();
                options.SchemaFilter<EnumSchemaFilter>();
                options.SupportNonNullableReferenceTypes();
                options.UseAllOfToExtendReferenceSchemas();
            });
            builder.Services.ConfigureHttpJsonOptions(o =>
            {
                o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull;
            });

            // In production, the React files will be served from this directory
            builder.Services.AddSpaStaticFiles(c => { c.RootPath = "spa"; });

            var app = builder.Build();
            await Initialize(app.Services);

            // app.UseSmtp();
            //////////////
            var cts = new CancellationTokenSource();
            var cert = System.Security.Cryptography.X509Certificates.X509Certificate2.CreateFromPemFile(C.Paths.CertCrt, C.Paths.CertKey);
            var opt = new Smtp.ServerOptions("abcd.ica.hr", cert);
            var server = new Smtp.Server(opt, app.Services);
            _ = server.StartAsync(cts.Token);
            /////////////
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
                app.UseForwardedHeaders();

            app.UseSpaStaticFiles();
            app.UseHttpsRedirection();
            // app.UseAuthentication();
            // app.UseAuthorization();

            app.MapGroup("/api").MapApi();

            app.MapWhen(x => !x.Request.Path.Value!.StartsWith("/api/"), builder =>
            {
                builder.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "../client-app"; // ???
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
        if (C.IsDebug)
        {
            Directory.CreateDirectory(C.Paths.MailData);
            Directory.CreateDirectory(C.Paths.ConfigData);
            Directory.CreateDirectory(C.Paths.CertData);
        }

        if (string.IsNullOrWhiteSpace(C.Hostname))
            throw new Exception("You must specify HOSTNAME environment variable");

        if (string.IsNullOrWhiteSpace(C.MasterUser) || string.IsNullOrWhiteSpace(C.MasterSecret))
            throw new Exception("You must specify MASTER_USER & MASTER_SECRET environment variables");

        if (!File.Exists(C.Paths.CertCrt) || !File.Exists(C.Paths.CertKey))
            throw new Exception($"Could not load certs from {C.Paths.CertData}");

        if (!Directory.GetFiles(C.Paths.ConfigData).Any())
            DovecotConfiguration.Initial();

        var dbFactory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var db = dbFactory.CreateDbContext();

        if (db.Database.GetMigrations().Any())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        // Demo data
        if (C.IsDebug && !db.Domains.Any())
        {
            var dpProvider = provider.GetRequiredService<IDataProtectionProvider>();
            await db.InitializeDefaults(dpProvider);
        }
    }
}

