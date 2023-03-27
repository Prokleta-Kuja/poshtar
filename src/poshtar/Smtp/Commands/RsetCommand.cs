namespace poshtar.Smtp.Commands;

public class RsetCommand : Command
{
    public const string Command = "RSET";

    /// <summary>
    /// Constructor.
    /// </summary>
    public RsetCommand() : base(Command) { }

    /// <summary>
    /// Execute the command.
    /// </summary>
    /// <param name="ctx">The execution context to operate on.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
    /// if the current state is to be maintained.</returns>
    internal override async Task<bool> ExecuteAsync(SessionContext ctx, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(ctx.Transaction.From))
            ctx.ResetTransaction();

        if (ctx.Pipe == null)
            return false;

        ctx.Log($"RSET - Transaction cleared");
        await ctx.Pipe.Output.WriteReplyAsync(Response.Ok, cancellationToken).ConfigureAwait(false);

        return true;
    }
}