using System.ComponentModel;
using System.Text.Json;
using Hangfire;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using poshtar.Entities;

namespace poshtar.Jobs;

public class ReturnEmail
{
    readonly ILogger<ReturnEmail> _logger;
    readonly AppDbContext _db;
    public ReturnEmail(ILogger<ReturnEmail> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }
    [DisplayName("Return Email - {0}")]
    [AutomaticRetry(Attempts = 0)]
    public async Task Run(Guid sessionId, PerformContext context, CancellationToken token)
    {
        var emlPath = C.Paths.QueueDataFor($"{sessionId}.eml");
        if (!File.Exists(emlPath))
        {
            _db.Logs.Add(new(sessionId, "Email not found, probably already returned", null));
            await _db.SaveChangesAsync(token);
            return;
        }

        var recipients = await _db.Recipients
            .Where(r => r.ContextId == sessionId)
            .OrderBy(r => r.UserId)
            .ToListAsync(token);

        if (recipients.Count == 0)
        {
            _db.Logs.Add(new(sessionId, "No recipient found, probably already delivered so deleting message", null));
            await _db.SaveChangesAsync(token);
            File.Delete(emlPath);
            return;
        }

        var internalUsers = new List<string>();
        List<string>? externalAddresses = null;
        foreach (var recipient in recipients)
        {
            if (recipient.UserId.HasValue)
                internalUsers.Add(recipient.Data);
            else
                externalAddresses = JsonSerializer.Deserialize<List<string>>(recipient.Data);
        }

        //TODO: find out who sender is
    }
}