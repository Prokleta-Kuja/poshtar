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
        Address = address;
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
            foreach (var internalUser in internalUsers)
                ctx.Transaction.ToUsers.TryAdd(internalUser.UserId, internalUser.Name);
        else if (ctx.IsSubmissionPort)
            ctx.Transaction.To.Add(Address);
        else
        {
            await ctx.Pipe.Output.WriteReplyAsync(Response.MailboxNameNotAllowed, cancellationToken).ConfigureAwait(false);
            return false;
        }

        await ctx.Pipe.Output.WriteReplyAsync(Response.Ok, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Gets the address that the mail is to.
    /// </summary>
    public EmailAddress Address { get; }
}