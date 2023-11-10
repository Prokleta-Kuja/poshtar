using Microsoft.EntityFrameworkCore;

namespace poshtar.Smtp.Commands;

public sealed class RcptCommand : Command
{
    public const string Command = "RCPT";

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="address">The address.</param>
    public RcptCommand(EmailAddress address) : base(Command)
    {
        Address = new(address.User.ToLower(), address.Host.ToLower());
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
        if (ctx.Pipe == null || ctx.Transaction.From == null)
            throw new NotSupportedException("The Acceptance state is not supported.");

        ctx.Log($"RCPT TO: {Address}");
        var internalUsers = await ctx.Db.Users
            .AsNoTracking()
            .Where(u => !u.Disabled.HasValue)
            .Where(u => u.Addresses.Any(a =>
                !a.Disabled.HasValue &&
                !a.Domain!.Disabled.HasValue &&
                a.Domain.Name.Equals(Address.Host.ToLower()) &&
                (a.Expression == null || EF.Functions.Like(Address.User, a.Expression))
                ))
             .ToListAsync(cancellationToken).ConfigureAwait(false);

        if (internalUsers.Count > 0)
        {
            var usernames = new List<string>(internalUsers.Count);
            foreach (var internalUser in internalUsers)
            {
                usernames.Add(internalUser.Name);
                ctx.Transaction.InternalUsers.TryAdd(internalUser.UserId, internalUser.Name);
            }
            ctx.Log($"Resolved to {string.Join(", ", usernames)}");
        }
        else if (ctx.IsSubmissionPort)
        {
            if (ctx.CanRelay)
            {
                ctx.Log("Not resolved to internal user(s), will relay");
                ctx.Transaction.ExternalAddresses.Add(Address.ToString());
            }
            else
            {
                ctx.Log("Not resolved to internal user(s), can't relay so refused");
                await ctx.Pipe.Output.WriteReplyAsync(Response.MailboxUnavailable, cancellationToken).ConfigureAwait(false);
                return false;
            }
        }
        else
        {
            ctx.Log("Refused recepient");
            await ctx.LocalRecipientNotResolvedAsnyc();
            await ctx.Pipe.Output.WriteReplyAsync(Response.MailboxNameNotAllowed, cancellationToken).ConfigureAwait(false);
            return false;
        }

        ctx.LocalRecipientResolved();
        await ctx.Pipe.Output.WriteReplyAsync(Response.Ok, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Gets the address that the mail is to.
    /// </summary>
    public EmailAddress Address { get; }
}