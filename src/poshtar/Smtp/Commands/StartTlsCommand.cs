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
    /// <param name="context">The execution context to operate on.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
    /// if the current state is to be maintained.</returns>
    internal override async Task<bool> ExecuteAsync(SessionContext context, CancellationToken cancellationToken)
    {
        if (context.Pipe != null)
            await context.Pipe.Output.WriteReplyAsync(Response.ServiceReady, cancellationToken).ConfigureAwait(false);

        var certificate = context.EndpointDefinition.ServerCertificate;
        var protocols = SslProtocols.Tls13 | SslProtocols.Tls12;

        if (context.Pipe != null)
            await context.Pipe.UpgradeAsync(certificate, protocols, cancellationToken).ConfigureAwait(false);

        return true;
    }
}