using Hangfire;
using Hangfire.Storage;
using poshtar.Jobs;

namespace poshtar.Extensions;

public static class RecurringJobs
{
    public static void ReregisterRecurringJobs(this IApplicationBuilder app)
    {
        var defaultOpt = new RecurringJobOptions
        {
            MisfireHandling = MisfireHandlingMode.Ignorable,
            TimeZone = C.TZ,
        };

        // Track active recurring jobs and add/update
        var activeJobIds = new HashSet<string>();

        activeJobIds.Add(nameof(CleanupTransactions));
        RecurringJob.AddOrUpdate<CleanupTransactions>(
           nameof(CleanupTransactions),
           j => j.Run(null, CancellationToken.None),
           "0 4 * * *", // Every day @ 4
           defaultOpt);

        activeJobIds.Add(nameof(CleanupBlockedIps));
        RecurringJob.AddOrUpdate<CleanupBlockedIps>(
           nameof(CleanupBlockedIps),
           j => j.Run(null, CancellationToken.None),
           "0 5 * * *", // Every day @ 5
           defaultOpt);

        activeJobIds.Add(nameof(CleanupCalendarObjects));
        RecurringJob.AddOrUpdate<CleanupCalendarObjects>(
           nameof(CleanupCalendarObjects),
           j => j.Run(null, CancellationToken.None),
           "0 3 * * *", // Every day @ 3
           defaultOpt);

        activeJobIds.Add(nameof(CertReload));
        RecurringJob.AddOrUpdate<CertReload>(
            nameof(CertReload),
            j => j.Run(CancellationToken.None),
            "55 3 * * SUN", // Every Sunday @ 3:55
            defaultOpt);

        // Get all registered recurring jobs
        var conn = JobStorage.Current.GetConnection();
        var jobs = conn.GetRecurringJobs();

        // Unregister recurring jobs no longer active
        foreach (var job in jobs)
            if (!activeJobIds.Contains(job.Id))
                RecurringJob.RemoveIfExists(job.Id);
    }
}