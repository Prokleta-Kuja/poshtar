using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Hangfire;
using Hangfire.Server;
using MailKit;
using MailKit.Net.Imap;
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
    public async Task Run(int transactionId, PerformContext context, CancellationToken token)
    {
        var transaction = await _db.Transactions
            .Include(t => t.Recipients.Where(r => !r.Delivered))
            .Include(t => t.FromUser)
            .SingleOrDefaultAsync(t => t.TransactionId == transactionId, token);

        if (transaction == null)
        {
            _logger.LogDebug("Could not find transaction with id {transactionId}", transactionId);
            return;
        }

        var emlPath = C.Paths.QueueDataFor($"{transactionId}.eml");
        if (!File.Exists(emlPath))
        {
            _db.Logs.Add(new("Email not found, probably already delivered"));
            await _db.SaveChangesAsync(token);
            return;
        }

        if (transaction.Recipients.Count == 0)
        {
            _db.Logs.Add(new("No recipients found, probably already delivered so deleting message"));
            await _db.SaveChangesAsync(token);
            File.Delete(emlPath);
            return;
        }

        var internalUsers = new List<string>();
        List<string>? externalAddresses = null;
        foreach (var recipient in transaction.Recipients)
        {
            if (recipient.UserId.HasValue)
                internalUsers.Add(recipient.Data);
            else
                externalAddresses = JsonSerializer.Deserialize<List<string>>(recipient.Data);
        }

        try
        {
            if (transaction.FromUser != null)
            {
                using var emlStream = File.OpenRead(emlPath);
                await Bounce(transaction.FromUser, transaction.ConnectionId, emlStream, internalUsers, externalAddresses, token);
                _db.Logs.Add(new($"Email bounced to user {transaction.FromUser.Name}"));
            }
            else
                _db.Logs.Add(new("Email is from external user, skipping sending bounce"));

            File.Delete(emlPath);
        }
        catch (Exception)
        {
            _db.Logs.Add(new("Email failed to bounce"));
        }
        finally
        {
            await _db.SaveChangesAsync(token);
        }
    }

    static async Task Bounce(User user, Guid connectionId, FileStream emlStream, List<string> internalUsers, List<string>? externalAddresses, CancellationToken token)
    {
        var mailerAddress = new MailboxAddress("Mail Delivery System", $"MAILER-DAEMON@{C.Hostname}");
        var recipient = new MailboxAddress("SENDER", $"{user.Name}@{C.Hostname}");
        var msg = new MimeMessage();
        msg.Headers.Add("Return-Path", "<>");
        msg.Headers.Add("Auto-Submitted", "auto-replied");
        msg.From.Add(mailerAddress);
        msg.To.Add(recipient);
        msg.Subject = "Undelivered Mail Returned to Sender";

        var sb = new StringBuilder($"This is the mail system at host {C.Hostname}.");
        sb.AppendLine(@$"

I'm sorry to have to inform you that your message could not
be delivered to one or more recipients. Message id: {connectionId}
");

        if (internalUsers.Count > 0)
            sb.AppendLine($"Internal users: {string.Join(", ", internalUsers)}");
        if (externalAddresses?.Count > 0)
            sb.AppendLine($"Addresses: {string.Join(", ", externalAddresses)}");

        sb.AppendLine(@"

For further assistance, please send mail to your postmaster.

If you do so, please include this problem report. You can
delete your own text from the attached returned message.");

        var bb = new BodyBuilder { TextBody = sb.ToString() };
        bb.Attachments.Add($"{connectionId}.eml", emlStream, ContentType.Parse("message/rfc822"), token);
        msg.Body = bb.ToMessageBody();

        using var client = new ImapClient();
        await client.ConnectAsync("localhost", C.Dovecot.PORT, true, token);
        await client.AuthenticateAsync($"{user.Name}*{C.Dovecot.MasterUser}", C.Dovecot.MasterPassword, token);

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadWrite, token);

        await inbox.AppendAsync(msg, MessageFlags.None, token);
        await inbox.CloseAsync(false, token);
        await client.DisconnectAsync(true, token);
    }
}