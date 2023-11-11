using System.ComponentModel;
using Hangfire;
using poshtar.Services;

namespace poshtar.Jobs;

public class CertReload
{
    readonly ILogger<CertReload> _logger;
    public CertReload(ILogger<CertReload> logger)
    {
        _logger = logger;
    }
    [DisplayName("Cert reload")]
    [AutomaticRetry(Attempts = 0)]
    public async Task Run(CancellationToken token)
    {
        _logger.LogInformation("Reloading server in case new certs were issued");

        SmtpService.Restart();
        await DovecotService.RestartAsync();
    }
}