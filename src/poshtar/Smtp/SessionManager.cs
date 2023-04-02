namespace poshtar.Smtp;

public class SessionManager
{
    readonly HashSet<SessionHandle> _sessions = new();
    readonly object _sessionsLock = new();

    internal void Run(SessionContext sessionContext, CancellationToken cancellationToken)
    {
        var handle = new SessionHandle(new Session(sessionContext), sessionContext);
        Add(handle);

        handle.CompletionTask = RunAsync(handle, cancellationToken);
        handle.CompletionTask.ContinueWith(task =>
            {
                Remove(handle);
            }, cancellationToken);
    }

    static async Task RunAsync(SessionHandle handle, CancellationToken cancellationToken)
    {
        try
        {
            handle.SessionContext.Transaction.Secure = await UpgradeAsync(handle, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            await handle.Session.RunAsync(cancellationToken);
        }
        catch (OperationCanceledException) { }
        catch (Exception) { }
        finally
        {
            await handle.SessionContext.Pipe!.Input.CompleteAsync();
            handle.SessionContext.Dispose();
        }
    }

    static async Task<bool> UpgradeAsync(SessionHandle handle, CancellationToken cancellationToken)
    {
        var endpoint = handle.SessionContext.EndpointDefinition;
        if (endpoint.Endpoint.Port == C.Smtp.IMPLICIT_SUBMISSION_PORT)
        {
            await handle.SessionContext.Pipe!.UpgradeAsync(endpoint.ServerCertificate, C.Smtp.TLS_PROTOCOLS, cancellationToken).ConfigureAwait(false);
            return true;
        }

        return false;
    }

    internal Task WaitAsync()
    {
        IReadOnlyList<Task> tasks;
        lock (_sessionsLock)
        {
            tasks = _sessions.Select(session => session.CompletionTask!).ToList();
        }

        return Task.WhenAll(tasks);
    }

    void Add(SessionHandle handle)
    {
        lock (_sessionsLock)
        {
            _sessions.Add(handle);
        }
    }

    void Remove(SessionHandle handle)
    {
        lock (_sessionsLock)
        {
            _sessions.Remove(handle);
        }
    }

    class SessionHandle
    {
        public SessionHandle(Session session, SessionContext sessionContext)
        {
            Session = session;
            SessionContext = sessionContext;
        }

        public Session Session { get; }
        public SessionContext SessionContext { get; }
        public Task? CompletionTask { get; set; }
    }
}