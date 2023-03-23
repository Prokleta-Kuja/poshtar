using System.Buffers;
using System.Net;
using poshtar.Entities;

namespace poshtar.Smtp;

public class SessionContext : IDisposable
{
    public Guid SessionId { get; } = Guid.NewGuid();
    public bool IsSubmissionPort;
    public IServiceScope ServiceScope { get; }
    public AppDbContext Db { get; }
    public EndpointDefinition EndpointDefinition { get; }
    public IPEndPoint? RemoteEndpoint { get; set; }
    public SecurableDuplexPipe? Pipe { get; set; }
    public MessageTransaction Transaction { get; }
    public User? User { get; set; }
    public bool IsAuthenticated => User != null;
    public bool IsQuitRequested { get; set; }
    public Dictionary<string, object> Properties { get; }
    public SessionContext(IServiceProvider serviceProvider, EndpointDefinition endpointDefinition)
    {
        ServiceScope = serviceProvider.CreateScope();
        EndpointDefinition = endpointDefinition;
        IsSubmissionPort = endpointDefinition.AuthenticationRequired;
        Transaction = new MessageTransaction();
        Properties = new();

        Db = ServiceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
    public void Log(string message, object? properties = null) => Db.Logs.Add(new(SessionId, message, properties));
    public Task<Response> SaveAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        // TODO: save or send email
        Console.WriteLine("Message pushed.");
        return Task.FromResult(Response.Ok);
    }
    public void Dispose()
    {
        Pipe?.Dispose();
        if (Db != null)
        {
            if (Db.ChangeTracker.HasChanges())
                Db.SaveChanges();
            Db.Dispose();
        }
        ServiceScope?.Dispose();
    }
}