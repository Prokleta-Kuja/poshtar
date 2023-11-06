using System.Net;
using System.Net.Sockets;

namespace poshtar.Smtp;

public class EndpointListener : IDisposable
{
    readonly EndpointDefinition _endpointDefinition;
    readonly TcpListener _tcpListener;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="endpointDefinition">The endpoint definition to create the listener for.</param>
    /// <param name="tcpListener">The TCP listener for the endpoint.</param>
    /// <param name="disposeAction">The action to execute when the listener has been disposed.</param>
    internal EndpointListener(EndpointDefinition endpointDefinition, TcpListener tcpListener)
    {
        _endpointDefinition = endpointDefinition;
        _tcpListener = tcpListener;
    }

    /// <summary>
    /// Returns a securable pipe to the endpoint.
    /// </summary>
    /// <param name="context">The session context that the pipe is being created for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The securable pipe from the endpoint.</returns>
    public async Task<SecurableDuplexPipe> GetPipeAsync(SessionContext context, CancellationToken cancellationToken)
    {
        var tcpClient = await _tcpListener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        if (tcpClient.Client.RemoteEndPoint is IPEndPoint ip)
        {
            context.RemoteEndpoint = ip;
            context.Transaction.Start = DateTime.UtcNow;
            context.Transaction.IpAddress = ip.Address.ToString();

            var info = context.IpSvc.GetInfo(context.Transaction.IpAddress);
            context.Transaction.CountryCode = info.code;
            context.Transaction.CountryName = info.name;
            context.Transaction.Asn = info.asn;
            context.Log($"Connection established with: {context.Transaction.IpAddress}");
            await AntiSpam.CheckAsnBlacklistAsync(context);
        }

        var stream = tcpClient.GetStream();
        stream.ReadTimeout = (int)_endpointDefinition.ReadTimeout.TotalMilliseconds;

        return new SecurableDuplexPipe(stream, () =>
        {
            tcpClient.Close();
            tcpClient.Dispose();
        });
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _tcpListener.Stop();
    }
}

public class EndpointListenerFactory
{
    public static readonly EndpointListenerFactory Default = new();

    /// <summary>
    /// Create an instance of an endpoint listener for the specified endpoint definition.
    /// </summary>
    /// <param name="endpointDefinition">The endpoint definition to create the listener for.</param>
    /// <returns>The endpoint listener for the specified endpoint definition.</returns>
    public virtual EndpointListener CreateListener(EndpointDefinition endpointDefinition)
    {
        var tcpListener = new TcpListener(endpointDefinition.Endpoint);
        tcpListener.Start();

        return new EndpointListener(endpointDefinition, tcpListener);
    }
}