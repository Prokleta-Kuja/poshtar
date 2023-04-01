using System.Net;
using poshtar.Entities;

namespace poshtar.Smtp;

public class SessionContext : IDisposable
{
    public Guid ConnectionId { get; set; } = Guid.NewGuid();
    public Transaction Transaction { get; private set; }
    public bool IsSubmissionPort;
    public bool CanRelay;
    public IServiceScope ServiceScope { get; }
    public AppDbContext Db { get; }
    public EndpointDefinition EndpointDefinition { get; }
    public IPEndPoint? RemoteEndpoint { get; set; }
    public SecurableDuplexPipe? Pipe { get; set; }
    public bool IsAuthenticated => Transaction.FromUser != null;
    public bool IsQuitRequested { get; set; }
    public Dictionary<string, object> Properties { get; }
    public SessionContext(IServiceProvider serviceProvider, EndpointDefinition endpointDefinition)
    {
        ServiceScope = serviceProvider.CreateScope();
        EndpointDefinition = endpointDefinition;
        IsSubmissionPort = endpointDefinition.AuthenticationRequired;
        Properties = new();

        Db = ServiceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Transaction = new() { ConnectionId = ConnectionId, Start = DateTime.MaxValue };
        Db.Transactions.Add(Transaction);
    }
    public void ResetTransaction()
    {
        Transaction.End = DateTime.UtcNow;
        Transaction = new()
        {
            ConnectionId = ConnectionId,
            Start = DateTime.UtcNow,
            Client = Transaction.Client,
            FromUser = Transaction.FromUser,
            FromUserId = Transaction.FromUserId,
        };
        Db.Transactions.Add(Transaction);
    }
    public void Log(string message, object? properties = null) => Transaction.Logs.Add(new(message, properties));
    public void Dispose()
    {
        Pipe?.Dispose();
        if (Db != null)
        {
            Transaction.End = DateTime.UtcNow;
            if (Db.ChangeTracker.HasChanges())
                Db.SaveChanges();
            Db.Dispose();
        }
        ServiceScope?.Dispose();
    }
}