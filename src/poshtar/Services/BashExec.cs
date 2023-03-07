using System.Diagnostics;

namespace poshtar.Services;

public static class BashExec
{
    static async Task<(string error, string output, int exitCode)> RunAsync(string cmd)
    {
        var args = C.IsDebug ? $"-c \"{cmd}\"" : $"-c \"sudo {cmd}\"";
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            FileName = "/bin/bash",
            Arguments = args,
        };
        var proc = new Process { StartInfo = startInfo, };
        proc.Start();
        await proc.WaitForExitAsync();

        var error = proc.StandardError.ReadToEnd();
        var output = proc.StandardOutput.ReadToEnd();
        return (error, output, proc.ExitCode);
    }

    // Dovecot
    public static async Task<(string error, string output, int exitCode)> CreateDovecotLog()
        => await RunAsync($"install -m 644 /dev/null /var/log/dovecot.log");
    public static async Task<(string error, string output, int exitCode)> StartDovecotAsync()
        => await RunAsync($"dovecot -c {DovecotConfiguration.MainPath}");
    public static async Task<(string error, string output, int exitCode)> RestartDovecotAsync()
        => await RunAsync($"dovecot -c {DovecotConfiguration.MainPath} reload");
    public static async Task<(string error, string output, int exitCode)> StopDovecotAsync()
        => await RunAsync($"dovecot stop");
    public static async Task<(string error, string output, int exitCode)> StatusDovecotAsync()
         => await RunAsync($"service dovecot status");

    // Postfix
    public static async Task<(string error, string output, int exitCode)> StartPostfixAsync()
        => await RunAsync($"postfix -c {PostfixConfiguration.PostfixRoot} start");
    public static async Task<(string error, string output, int exitCode)> RestartPostfixAsync()
        => await RunAsync($"postfix -c {PostfixConfiguration.PostfixRoot} reload");
    public static async Task<(string error, string output, int exitCode)> StopPostfixAsync()
        => await RunAsync($"postfix stop");
    public static async Task<(string error, string output, int exitCode)> StatusPostfixAsync()
        => await RunAsync($"service postfix status");
}