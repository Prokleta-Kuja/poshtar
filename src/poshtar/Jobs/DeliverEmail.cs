using System.ComponentModel;
using System.Text.Json;
using Hangfire;
using Hangfire.Server;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using poshtar.Entities;

namespace poshtar.Jobs;

// [AutomaticRetry(Attempts = 5, DelayInSecondsByAttemptFunc =)]
public class DeliverEmail
{
    readonly ILogger<DeliverEmail> _logger;
    readonly AppDbContext _db;
    readonly IBackgroundJobClient _job;
    public DeliverEmail(ILogger<DeliverEmail> logger, AppDbContext db, IBackgroundJobClient job)
    {
        _logger = logger;
        _db = db;
        _job = job;
    }

    [DisplayName("Deliver Email - {0}")]
    public async Task Run(Guid sessionId, PerformContext context, CancellationToken token)
    {

        var emlPath = C.Paths.QueueDataFor($"{sessionId}.eml");
        if (!File.Exists(emlPath))
        {
            _db.Logs.Add(new(sessionId, "Email not found, probably already delivered", null));
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

        var errors = 0;
        var msg = await MimeMessage.LoadAsync(emlPath, token);
        foreach (var recipient in recipients)
        {
            try
            {
                if (recipient.UserId.HasValue)
                {
                    await Deliver(msg, recipient.Data, token);
                    _db.Logs.Add(new(sessionId, $"Delivered to user {recipient.Data}", null));
                    _db.Recipients.Remove(recipient);
                }
                else
                {
                    var to = JsonSerializer.Deserialize<List<string>>(recipient.Data);
                    if (to == null)
                    {
                        _db.Logs.Add(new(sessionId, "External recipients could not be deserialized", null));
                        throw new Exception("External recipients could not be deserialized");
                    }
                    else
                    {
                        await Forward(msg, to, token);
                        _db.Logs.Add(new(sessionId, "Forwarded for external addresses", recipient.Data));
                        _db.Recipients.Remove(recipient);
                    }
                }
            }
            catch (Exception)
            {
                errors++;
            }
        }

        if (_db.ChangeTracker.HasChanges())
            await _db.SaveChangesAsync(token);

        if (errors == 0)
            File.Delete(emlPath);
        else
        {
            var retryCount = context.GetJobParameter<int?>("RetryCount");
            if (retryCount.HasValue && retryCount == 10)
            {
                _job.Enqueue<ReturnEmail>(j => j.Run(sessionId, null!, CancellationToken.None));
                return;
            }

            throw new Exception("Could not deliver message to all recipients");
        }
    }
    async Task Deliver(MimeMessage msg, string toUser, CancellationToken token)
    {
        using var client = new ImapClient();
        await client.ConnectAsync("localhost", 993, true, token);
        await client.AuthenticateAsync($"{toUser}*{C.Dovecot.MasterUser}", C.Dovecot.MasterPassword, token);

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadWrite, token);

        await inbox.AppendAsync(msg, MessageFlags.None, token);
        await inbox.CloseAsync(false, token);
        await client.DisconnectAsync(true, token);
    }
    async Task Forward(MimeMessage msg, IEnumerable<string> to, CancellationToken token)
    {
        if (msg.From.FirstOrDefault() is not MailboxAddress sender)
            throw new Exception("Could not parse sender");

        var domain = await _db.Domains.SingleAsync(d => d.Name == sender.Domain, token);
        var recipients = to.Select(r => MailboxAddress.Parse(r));

        using var client = new SmtpClient();
        await client.ConnectAsync(domain.Host, domain.Port, SecureSocketOptions.Auto, token);
        await client.AuthenticateAsync(domain.Username, domain.Password, token);
        await client.SendAsync(msg, sender, recipients, token);
        await client.DisconnectAsync(true, token);
    }
}