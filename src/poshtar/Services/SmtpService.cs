using System.Security.Cryptography.X509Certificates;

namespace poshtar.Services;

public static class SmtpService
{
    static IServiceProvider? s_provider;
    static poshtar.Smtp.Server? s_server;
    static CancellationTokenSource s_cts = new();
    static Task? s_serverTask;
    public static void AddSmtp(this IServiceCollection services) { }
    public static void UseSmtp(this WebApplication app)
    {
        s_provider = app.Services;
        Start();

        var appLifetime = s_provider.GetRequiredService<IHostApplicationLifetime>();
        appLifetime.ApplicationStopping.Register(Stop);
    }
    static CancellationToken GetToken()
    {
        if (s_cts.IsCancellationRequested)
        {
            s_cts.Dispose();
            s_cts = new();
        }

        return s_cts.Token;
    }
    static void Start()
    {
        if (s_provider == null)
            throw new Exception("SMTP service not initialized via UseSmtp");

        var token = GetToken();

        var cert = X509Certificate2.CreateFromPemFile(C.Paths.CertCrt, C.Paths.CertKey);
        s_server = new poshtar.Smtp.Server(s_provider);
        s_serverTask = s_server.StartAsync(cert, token);
    }
    static void Stop()
    {
        s_server?.Shutdown();
        try
        {
            s_server?.ShutdownTask?.Wait();
        }
        catch (AggregateException e)
        {
            e.Handle(exception => exception is OperationCanceledException);
        }
        try
        {
            s_serverTask?.Wait();
        }
        catch (AggregateException e)
        {
            e.Handle(exception => exception is OperationCanceledException);
        }
    }
    static void Restart()
    {
        Stop();
        Start();
    }
}