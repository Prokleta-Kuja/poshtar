using System.ComponentModel;
using System.Text.Json;
using Hangfire;
using Hangfire.Server;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using poshtar.Entities;

namespace poshtar.Jobs;

// [AutomaticRetry(Attempts = 5, DelayInSecondsByAttemptFunc =)]
public class DeliverEmail
{
    readonly ILogger<DeliverEmail> _logger;
    readonly AppDbContext _db;
    readonly IDataProtectionProvider _dpp;
    readonly IBackgroundJobClient _job;
    public DeliverEmail(ILogger<DeliverEmail> logger, AppDbContext db, IDataProtectionProvider dpp, IBackgroundJobClient job)
    {
        _logger = logger;
        _db = db;
        _dpp = dpp;
        _job = job;
    }

    [DisplayName("Deliver Email - {0}")]
    public async Task Run(int transactionId, PerformContext context, CancellationToken token)
    {
        var transaction = await _db.Transactions
            .Include(t => t.Recipients.Where(r => !r.Delivered))
            .SingleOrDefaultAsync(t => t.TransactionId == transactionId, token);

        if (transaction == null)
        {
            _logger.LogDebug("Could not find transaction with id {transactionId}", transactionId);
            return;
        }

        var emlPath = C.Paths.QueueDataFor($"{transactionId}.eml");
        if (!File.Exists(emlPath))
        {
            transaction.Logs.Add(new("Email not found, probably already delivered"));
            await _db.SaveChangesAsync(token);
            return;
        }

        if (transaction.Recipients.Count == 0)
        {
            transaction.Logs.Add(new("No recipients found, probably already delivered so deleting message"));
            await _db.SaveChangesAsync(token);
            File.Delete(emlPath);
            return;
        }

        var errors = 0;
        var msg = await MimeMessage.LoadAsync(emlPath, token);
        foreach (var recipient in transaction.Recipients)
        {
            try
            {
                if (recipient.UserId.HasValue)
                {
                    await Deliver(msg, recipient.Data, token);
                    transaction.Logs.Add(new($"Delivered to user {recipient.Data}"));
                    recipient.Delivered = true;
                }
                else
                {
                    var to = JsonSerializer.Deserialize<List<string>>(recipient.Data);
                    if (to == null)
                    {
                        transaction.Logs.Add(new("External recipients could not be deserialized"));
                        throw new Exception("External recipients could not be deserialized");
                    }
                    else
                    {
                        await Forward(msg, to, token);
                        transaction.Logs.Add(new("Forwarded for external addresses", recipient.Data));
                        recipient.Delivered = true;
                    }
                }
            }
            catch (Exception ex)
            {
                errors++;
                if (recipient.UserId.HasValue)
                    transaction.Logs.Add(new($"Failed to process recipient {recipient.Data}: {ex.Message}"));
                else
                    transaction.Logs.Add(new($"Failed to forward message: {ex.Message}"));
            }
            finally
            {
                await _db.SaveChangesAsync(token);
            }
        }

        if (errors == 0)
            File.Delete(emlPath);
        else
        {
            var retryCount = context.GetJobParameter<int?>("RetryCount");
            if (retryCount.HasValue && retryCount == 10)
            {
                _job.Enqueue<ReturnEmail>(j => j.Run(transactionId, null!, CancellationToken.None));
                return;
            }

            throw new Exception("Could not deliver message to all recipients");
        }
    }
    static async Task Deliver(MimeMessage msg, string toUser, CancellationToken token)
    {
        using var client = new ImapClient();
        await client.ConnectAsync(C.Dovecot.INTERNAL_HOST, C.Dovecot.INSECURE_PORT, SecureSocketOptions.None, token);
        // TODO: test
        // await client.AuthenticateAsync($"{toUser}*nanadmin", "admin", token);
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

        var recipients = to.Select(r => MailboxAddress.Parse(r));
        var domain = await _db.Domains.SingleAsync(d => d.Name == sender.Domain, token);
        var serverProtector = _dpp.CreateProtector(nameof(Domain));

        using var client = new SmtpClient();
        await client.ConnectAsync(domain.Host, domain.Port, SecureSocketOptions.Auto, token);
        await client.AuthenticateAsync(domain.Username, serverProtector.Unprotect(domain.Password), token);
        await client.SendAsync(msg, sender, recipients, token);
        await client.DisconnectAsync(true, token);
    }
}