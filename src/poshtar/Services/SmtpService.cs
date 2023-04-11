using System.Security.Cryptography.X509Certificates;
using Serilog;

namespace poshtar.Services;

public static class SmtpService
{
    static IServiceProvider? s_provider;
    static Smtp.Server? s_server;
    static CancellationTokenSource s_cts = new();
    static Task? s_serverTask;
    static readonly TimeSpan s_maxWait = TimeSpan.FromSeconds(5);
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
        s_server = new Smtp.Server(s_provider);
        s_serverTask = s_server.StartAsync(cert, token);
        Log.Information("SMTP server started");
    }
    static void Stop()
    {
        s_server?.Shutdown();
        try
        {
            var listenerComplete = s_server?.ShutdownTask?.Wait(s_maxWait);
            if (!listenerComplete ?? false)
            {
                Log.Warning("SMTP server listeners didn't shut down in timely manner");
                s_cts.Cancel();
            }
        }
        catch (AggregateException e)
        {
            e.Handle(exception => exception is OperationCanceledException);
        }
        try
        {
            var sessionsComplete = s_serverTask?.Wait(s_maxWait);
            if (!sessionsComplete ?? false)
            {
                Log.Warning("SMTP server sessions didn't shut down in timely manner");
                s_cts.Cancel();
            }
        }
        catch (AggregateException e)
        {
            e.Handle(exception => exception is OperationCanceledException);
        }
    }
    public static void Restart()
    {
        Stop();
        Start();
    }
}