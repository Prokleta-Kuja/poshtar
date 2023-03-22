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

        if (ctx.IsSubmissionPort)
        {
            // check if internal rcpt and remember userids
            // else accept everything
        }
        else
        {
            // accept only internal rcpt and remember userids
        }

        ctx.Transaction.To.Add(Address);
        await ctx.Pipe.Output.WriteReplyAsync(Response.Ok, cancellationToken).ConfigureAwait(false);
        return true;
        //     switch (await context.CanDeliverToAsync(Address, context.Transaction.From, cancellationToken).ConfigureAwait(false))
        //     {
        //         case MailboxFilterResult.Yes:
        //             context.Transaction.To.Add(Address);
        //             await context.Pipe.Output.WriteReplyAsync(Response.Ok, cancellationToken).ConfigureAwait(false);
        //             return true;

        //         case MailboxFilterResult.NoTemporarily:
        //             await context.Pipe.Output.WriteReplyAsync(Response.MailboxUnavailable, cancellationToken).ConfigureAwait(false);
        //             return false;

        //         case MailboxFilterResult.NoPermanently:
        //             await context.Pipe.Output.WriteReplyAsync(Response.MailboxNameNotAllowed, cancellationToken).ConfigureAwait(false);
        //             return false;
        //     }

        // throw new NotSupportedException("The Acceptance state is not supported.");
    }

    /// <summary>
    /// Gets the address that the mail is to.
    /// </summary>
    public EmailAddress Address { get; }
}