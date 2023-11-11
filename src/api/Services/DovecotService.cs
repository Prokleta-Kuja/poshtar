using Serilog;

namespace poshtar.Services;

public static class DovecotService
{
    public static async Task UseDovecotAsync(this WebApplication app)
    {
        if (C.StartApiOnly)
            return;

        var idChange = await BashExec.ChangeDovecotUidGid();
        if (idChange.exitCode == 0)
            Log.Debug("Changed dovecot permissions");
        else
            Log.Error("Could not change dovecot permissions: {error}", idChange.error);

        var dovecot = await BashExec.StartDovecotAsync();
        if (dovecot.exitCode == 0)
            Log.Debug("Dovecot started");
        else if (C.IsDebug)
        {
            dovecot = await BashExec.RestartDovecotAsync();
            if (dovecot.exitCode == 0)
                Log.Debug("Dovecot re-started");
            else
                Log.Error("Could not re-start dovecot: {error}", dovecot.error);
        }
        else
            Log.Error("Could not start dovecot: {error}", dovecot.error);

        var appLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        appLifetime.ApplicationStopping.Register(Stop);
    }
    public static async Task RestartAsync()
    {
        await BashExec.RestartDovecotAsync();
    }
    static void Stop()
    {
        _ = BashExec.StopDovecotAsync();
    }

}