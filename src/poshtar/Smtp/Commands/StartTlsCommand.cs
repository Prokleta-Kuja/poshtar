using System.Security.Authentication;

namespace poshtar.Smtp.Commands;

public class StartTlsCommand : Command
{
    public const string Command = "STARTTLS";

    /// <summary>
    /// Constructor.
    /// </summary>
    public StartTlsCommand() : base(Command) { }

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

        ctx.Log($"STARTTLS requested");
        await ctx.Pipe.Output.WriteReplyAsync(Response.ServiceReady, cancellationToken).ConfigureAwait(false);
        var certificate = ctx.EndpointDefinition.ServerCertificate;
        var protocols = SslProtocols.Tls13 | SslProtocols.Tls12;

        await ctx.Pipe.UpgradeAsync(certificate, protocols, cancellationToken).ConfigureAwait(false);
        ctx.Transaction.Secure = true;
        return true;
    }
}