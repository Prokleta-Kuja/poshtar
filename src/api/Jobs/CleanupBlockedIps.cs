using System.ComponentModel;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Jobs;

public class CleanupBlockedIps
{
    readonly ILogger<CleanupBlockedIps> _logger;
    readonly AppDbContext _db;
    public CleanupBlockedIps(ILogger<CleanupBlockedIps> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }
    [DisplayName("Cleanup Blocked IPs")]
    [AutomaticRetry(Attempts = 0)]
    public async Task Run(DateTime? before, CancellationToken token)
    {
        if (!before.HasValue)
            before = DateTime.UtcNow - TimeSpan.FromHours(C.Smtp.AntiSpamSettings.BanHours);

        _logger.LogInformation("Cleaning up blocked ip(s) before {Date}", before);
        var affected = await _db.BlockedIps.Where(b => b.LastHit < before).ExecuteDeleteAsync(token);
        _logger.LogInformation("Cleaned up {Count} blocked ip(s)", affected);
    }
}