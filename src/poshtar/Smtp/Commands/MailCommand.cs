using System.Net;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Spf;
using Microsoft.EntityFrameworkCore;

namespace poshtar.Smtp.Commands;
public enum MailboxFilterResult { Yes = 0, NoTemporarily = 1, NoPermanently = 2, SizeLimitExceeded = 3 }
public class MailCommand : Command
{
    public const string Command = "MAIL";

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="parameters">The list of extended (ESMTP) parameters.</param>
    public MailCommand(EmailAddress address, IReadOnlyDictionary<string, string> parameters) : base(Command)
    {
        Address = address;
        Parameters = parameters;
    }

    /// <summary>
    /// Execute the command.
    /// </summary>
    /// <param name="ctx">The execution context to operate on.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
    /// if the current state is to be maintained.</returns>
    internal override async Task<bool> ExecuteAsync(SessionContext ctx, CancellationToken cancellationToken)
    {
        if (ctx.Pipe == null)
            return false;

        // check if a size has been defined
        var size = GetMessageSize();
        var sizeLog = size == default ? string.Empty : $" SIZE={size} ({size / 1024 / 1024:0.00}MB)";

        if (!string.IsNullOrWhiteSpace(ctx.Transaction.From))
            ctx.ResetTransaction();

        ctx.Log($"MAIL FROM:{Address}{sizeLog}");
        ctx.Transaction.From = Address.ToString();

        // check against the server supplied maximum
        if (C.MaxMessageSize > 0 && size > C.MaxMessageSize)
        {
            ctx.Log($"Message size limit exceeded", new { size, limit = C.MaxMessageSize });
            await ctx.Pipe.Output.WriteReplyAsync(Response.SizeLimitExceeded, cancellationToken).ConfigureAwait(false);
            return false;
        }

        if (ctx.IsSubmissionPort)
        {
            if (!ctx.IsAuthenticated)
            {
                ctx.Log($"Refused mail from, authentication required");
                await ctx.Pipe.Output.WriteReplyAsync(Response.AuthenticationRequired, cancellationToken).ConfigureAwait(false);
                return false;
            }

            var canSend = await ctx.Db.Addresses
                .AsNoTracking()
                .Where(a => !a.Disabled.HasValue && (a.Expression == null || EF.Functions.Like(Address.User, a.Expression)))
                .Where(a => a.Users.Where(u => !u.Disabled.HasValue).Contains(ctx.Transaction.FromUser))
                .Where(a => a.Domain!.Name.Equals(Address.Host.ToLower()) && !a.Domain.Disabled.HasValue)
                .AnyAsync(cancellationToken).ConfigureAwait(false);

            if (!canSend)
            {
                ctx.Log("Sending as address not allowed");
                await ctx.Pipe.Output.WriteReplyAsync(Response.MailboxNameNotAllowed, cancellationToken).ConfigureAwait(false);
                return false;
            }

            var senderDomain = await ctx.Db.Domains
                .AsNoTracking()
                .SingleOrDefaultAsync(d => d.Name.Equals(Address.Host.ToLower()), cancellationToken);

            ctx.CanRelay = senderDomain?.RelayId.HasValue ?? false;
        }
        else
        {
            if (IPAddress.TryParse(ctx.Transaction.IpAddress, out var ip) && DomainName.TryParse(Address.Host, out var domain))
            {
                var validator = new SpfValidator();
                ctx.Spf = validator.CheckHost(ip, domain!, string.Empty).Result;
            }
            switch (ctx.Spf)
            {
                case SpfQualifier.Pass:
                    ctx.Log($"SPF {ctx.Spf}");
                    break;
                case SpfQualifier.Fail:
                case SpfQualifier.SoftFail:  //SPF softfail is interpreted in DMARC as fail by default
                case SpfQualifier.None:  //SPF none is treated as fail in DMARC
                case SpfQualifier.Neutral:  //SPF neutral is interpreted in DMARC as fail by default
                case SpfQualifier.TempError:  //the error is used to return a 4xx status code and the SMTP session ends
                case SpfQualifier.PermError:  //SPF permerror is interpreted in DMARC as fail
                    ctx.Log($"SPF {ctx.Spf}. Closing connection");
                    throw new ResponseException(Response.ServiceClosingTransmissionChannel, true);
            }
        }

        await ctx.Pipe.Output.WriteReplyAsync(Response.Ok, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Gets the estimated message size supplied from the ESMTP command extension.
    /// </summary>
    /// <returns>The estimated message size that was supplied by the client.</returns>
    int GetMessageSize()
    {
        if (Parameters.TryGetValue("SIZE", out var value) == false)
            return 0;

        return int.TryParse(value, out var size) == false ? 0 : size;
    }

    /// <summary>
    /// Gets the address that the mail is from.
    /// </summary>
    public EmailAddress Address { get; }

    /// <summary>
    /// The list of extended mail parameters.
    /// </summary>
    public IReadOnlyDictionary<string, string> Parameters { get; }
}