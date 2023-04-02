using System.Security.Cryptography.X509Certificates;

namespace poshtar.Smtp;

public class Server
{
    readonly IServiceProvider _serviceProvider;
    readonly EndpointListenerFactory _endpointListenerFactory;
    readonly SessionManager _sessions;
    readonly CancellationTokenSource _shutdownTokenSource = new();
    readonly TaskCompletionSource<bool> _shutdownTask = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="options">The SMTP server options.</param>
    /// <param name="serviceProvider">The service provider to use when resolving services.</param>
    public Server(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _sessions = new();
        _endpointListenerFactory = new();
    }

    /// <summary>
    /// Starts the SMTP server.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task which performs the operation.</returns>
    public async Task StartAsync(X509Certificate2 cert, CancellationToken cancellationToken)
    {
        var smtpEndpoint = new EndpointDefinition(C.Smtp.RELAY_PORT, cert);
        var implicitSubmissionEndpoint = new EndpointDefinition(C.Smtp.IMPLICIT_SUBMISSION_PORT, cert);
        var explicitSubmissionEndpoint = new EndpointDefinition(C.Smtp.EXPLICIT_SUBMISSION_PORT, cert);

        var tasks = new List<Task>
        {
            ListenAsync(smtpEndpoint, cancellationToken),
            ListenAsync(implicitSubmissionEndpoint, cancellationToken),
            ListenAsync(explicitSubmissionEndpoint, cancellationToken),
        };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        _shutdownTask.TrySetResult(true);

        await _sessions.WaitAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Shutdown the server and allow any active sessions to finish.
    /// </summary>
    public void Shutdown()
    {
        _shutdownTokenSource.Cancel();
    }

    /// <summary>
    /// Listen for SMTP traffic on the given endpoint.
    /// </summary>
    /// <param name="endpointDefinition">The definition of the endpoint to listen on.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task which performs the operation.</returns>
    async Task ListenAsync(EndpointDefinition endpointDefinition, CancellationToken cancellationToken)
    {
        // The listener can be stopped either by the caller cancelling the CancellationToken used when starting the server, or when calling
        // the shutdown method. The Shutdown method will stop the listeners and allow any active sessions to finish gracefully.
        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_shutdownTokenSource.Token, cancellationToken);

        using var endpointListener = _endpointListenerFactory.CreateListener(endpointDefinition);

        while (cancellationTokenSource.Token.IsCancellationRequested == false)
        {
            var sessionContext = new SessionContext(_serviceProvider, endpointDefinition);

            try
            {
                // wait for a client connection
                sessionContext.Pipe = await endpointListener.GetPipeAsync(sessionContext, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            catch (Exception)
            {
                continue;
            }

            if (sessionContext.Pipe != null)
                _sessions.Run(sessionContext, cancellationTokenSource.Token);
        }
    }

    /// <summary>
    /// The task that completes when the server has shutdown and stopped accepting new sessions.
    /// </summary>
    public Task ShutdownTask => _shutdownTask.Task;
}