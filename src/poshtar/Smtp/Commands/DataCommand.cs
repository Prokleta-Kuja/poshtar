namespace poshtar.Smtp.Commands;

public class DataCommand : Command
{
    public const string Command = "DATA";

    /// <summary>
    /// Constructor.
    /// </summary>
    public DataCommand() : base(Command) { }

    /// <summary>
    /// Execute the command.
    /// </summary>
    /// <param name="context">The execution context to operate on.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
    /// if the current state is to be maintained.</returns>
    internal override async Task<bool> ExecuteAsync(SessionContext context, CancellationToken cancellationToken)
    {
        if (context.Pipe == null)
            return false;

        if (context.Transaction.To.Count == 0)
        {
            await context.Pipe.Output.WriteReplyAsync(Response.NoValidRecipientsGiven, cancellationToken).ConfigureAwait(false);
            return false;
        }

        await context.Pipe.Output.WriteReplyAsync(new Response(ReplyCode.StartMailInput, "end with <CRLF>.<CRLF>"), cancellationToken).ConfigureAwait(false);

        try
        {
            Response? response = null;
            await context.Pipe.Input.ReadDotBlockAsync(
                async buffer =>
                {
                    response = await context.SaveAsync(buffer, cancellationToken).ConfigureAwait(false);
                },
                cancellationToken).ConfigureAwait(false);

            if (response != null)
                await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            await context.Pipe.Output.WriteReplyAsync(new Response(ReplyCode.TransactionFailed), cancellationToken).ConfigureAwait(false);
        }

        return true;
    }
}