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

        // Mark calendar objects exipred 14 days ago as deleted
        var now = DateTime.UtcNow;
        var markAffected = await _db.CalendarObjects
            .Where(co => !co.Deleted.HasValue && co.LastOccurence.HasValue && co.LastOccurence.Value < before)
            .ExecuteUpdateAsync(setters => setters.SetProperty(co => co.Deleted, now).SetProperty(co => co.Modified, now), token);
        if (markAffected > 0)
            _logger.LogInformation("Marked {Count} expired calendar object(s) as deleted", markAffected);

        // Permanently delete calendar objects marked for deletion 14 days ago
        System.Linq.Expressions.Expression<Func<CalendarObject, bool>> calendarObjDeletedBefore = co => co.Deleted.HasValue && co.Deleted < before;
        var fileNames = await _db.CalendarObjects.Where(calendarObjDeletedBefore).Select(co => co.FileName).ToListAsync(token);
        foreach (var fileName in fileNames)
            File.Delete(C.Paths.CalendarObjectsDataFor(fileName));

        var deleteAffected = await _db.CalendarObjects.Where(calendarObjDeletedBefore).ExecuteDeleteAsync();
        if (deleteAffected > 0)
            _logger.LogInformation("Permanently deleted {Count} calendar object(s)", deleteAffected);
    }
}