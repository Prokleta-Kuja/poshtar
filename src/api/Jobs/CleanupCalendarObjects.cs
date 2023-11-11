using System.ComponentModel;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Jobs;

public class CleanupCalendarObjects
{
    readonly ILogger<CleanupCalendarObjects> _logger;
    readonly AppDbContext _db;
    public CleanupCalendarObjects(ILogger<CleanupCalendarObjects> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }
    [DisplayName("Cleanup Calendar Objects")]
    [AutomaticRetry(Attempts = 0)]
    public async Task Run(DateTime? before, CancellationToken token)
    {
        if (!before.HasValue)
            before = DateTime.UtcNow.Date.AddDays(-14);

        _logger.LogInformation("Cleaning up calendar object(s) before {Date}", before);
        var fileNames = await _db.CalendarObjects.Where(co => co.Deleted.HasValue && co.Deleted < before).Select(co => co.FileName).ToListAsync();
        foreach (var fileName in fileNames)
            File.Delete(C.Paths.CalendarObjectsDataFor(fileName));

        var affected = await _db.CalendarObjects.Where(co => co.Deleted.HasValue && co.Deleted < before).ExecuteDeleteAsync(token);
        _logger.LogInformation("Cleaned up {Count} calendar object(s)", affected);
    }
}