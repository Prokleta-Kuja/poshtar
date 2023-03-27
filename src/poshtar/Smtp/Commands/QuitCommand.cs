namespace poshtar.Smtp.Commands;

public class QuitCommand : Command
{
    public const string Command = "QUIT";

    /// <summary>
    /// Constructor.
    /// </summary>
    public QuitCommand() : base(Command) { }

    /// <summary>
    /// Execute the command.
    /// </summary>
    /// <param name="ctx">The execution context to operate on.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
    /// if the current state is to be maintained.</returns>
    internal override async Task<bool> ExecuteAsync(SessionContext ctx, CancellationToken cancellationToken)
    {
        // TODO: Sending quit is a very small indicator not to be spam
        ctx.IsQuitRequested = true;

        if (ctx.Pipe != null)
            await ctx.Pipe.Output.WriteReplyAsync(Response.ServiceClosingTransmissionChannel, cancellationToken).ConfigureAwait(false);

        ctx.Log($"QUIT");
        return true;
    }
}