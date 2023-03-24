using System.ComponentModel;
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
    public DeliverEmail(ILogger<DeliverEmail> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [DisplayName("Deliver Email - {0}")]
    public void Run(Guid sessionId, PerformContext context, CancellationToken token)
    {
        // var retryCount = context.GetJobParameter<string>("RetryCount");

        var emlPath = C.Paths.QueueDataFor($"{sessionId}.eml");
        if (!File.Exists(emlPath))
        {
            // TODO: log
            return;
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
            throw new Exception("aaaa");

        var domain = await _db.Domains.SingleAsync(d => d.Name == sender.Domain, token);
        var recipients = to.Select(r => MailboxAddress.Parse(r));

        using var client = new SmtpClient();
        await client.ConnectAsync(domain.Host, domain.Port, SecureSocketOptions.Auto, token);
        await client.AuthenticateAsync(domain.Username, domain.Password, token);
        await client.SendAsync(msg, sender, recipients, token);
        await client.DisconnectAsync(true, token);
    }
}