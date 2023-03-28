using System.ComponentModel;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Jobs;

public class CleanupTransactions
{
    readonly ILogger<CleanupTransactions> _logger;
    readonly AppDbContext _db;
    public CleanupTransactions(ILogger<CleanupTransactions> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }
    [DisplayName("Cleanup Transactions")]
    [AutomaticRetry(Attempts = 0)]
    public async Task Run(DateTime? before, CancellationToken token)
    {
        if (!before.HasValue)
            before = DateTime.UtcNow.Date.AddDays(-14);

        _logger.LogInformation("Cleaning up transaction(s) before {Date}", before);
        var affected = await _db.Transactions.Where(t => t.Start < before).ExecuteDeleteAsync(token);
        _logger.LogInformation("Cleaned up {Count} transaction(s)", affected);

        // TODO: delete eml files
    }
}