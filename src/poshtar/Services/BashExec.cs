using System.Diagnostics;
using System.Text;

namespace poshtar.Services;

public static class BashExec
{
    static async Task<(int exitCode, string error, string output)> RunAsync(string cmd)
    {
        var args = C.IsDebug ? $"-c \"sudo {cmd}\"" : $"-c \"{cmd}\"";
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            FileName = "/bin/bash",
            Arguments = args,
        };
        using var proc = new Process { StartInfo = startInfo, };
        proc.Start();

        var cts = new CancellationTokenSource();
        using var outputStream = new MemoryStream();
        using var errorStream = new MemoryStream();
        var outputTask = proc.StandardOutput.BaseStream.CopyToAsync(outputStream, cts.Token);
        var errorTask = proc.StandardError.BaseStream.CopyToAsync(errorStream, cts.Token);

        await proc.WaitForExitAsync();
        proc.StandardOutput.BaseStream.Flush();
        proc.StandardError.BaseStream.Flush();
        cts.Cancel();

        var output = Encoding.UTF8.GetString(outputStream.ToArray());
        var error = Encoding.UTF8.GetString(errorStream.ToArray());

        return (proc.ExitCode, error, output);
    }

    // Dovecot
    public static async Task<(int exitCode, string error, string output)> StartDovecotAsync()
        => await RunAsync($"dovecot -c {DovecotConfiguration.MainPath}");
    public static async Task<(int exitCode, string error, string output)> RestartDovecotAsync()
        => await RunAsync($"dovecot -c {DovecotConfiguration.MainPath} reload");
    public static async Task<(int exitCode, string error, string output)> StopDovecotAsync()
        => await RunAsync($"dovecot stop");
    public static async Task<(int exitCode, string error, string output)> StatusDovecotAsync()
         => await RunAsync($"service dovecot status");
    public static async Task<(int exitCode, string error, string output)> ChangeDovecotUidGid()
         => await RunAsync($"usermod -u {C.Uid} vmail && groupmod -g {C.Gid} vmail");
}