namespace poshtar.Smtp.Commands;

public class HeloCommand : Command
{
    public const string Command = "HELO";

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="domainOrAddress">The domain name.</param>
    public HeloCommand(string domainOrAddress) : base(Command)
    {
        DomainOrAddress = domainOrAddress;
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

        ctx.Log($"HELO {DomainOrAddress}");
        ctx.Transaction.Client = DomainOrAddress;
        var response = new Response(ReplyCode.Ok, GetGreeting(ctx));
        await ctx.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Returns the greeting to display to the remote host.
    /// </summary>
    /// <param name="ctx">The session context.</param>
    /// <returns>The greeting text to display to the remote host.</returns>
    protected virtual string GetGreeting(SessionContext ctx)
    {
        if (ctx.IsSubmissionPort)
            return $"{C.Hostname} Hello {DomainOrAddress}, what do you want to send today?";
        else
            return $"{C.Hostname} Hello {DomainOrAddress}, got any emails for me?";
    }

    /// <summary>
    /// Gets the domain name.
    /// </summary>
    public string DomainOrAddress { get; }
}