using System.Net;
using ARSoft.Tools.Net.Spf;
using poshtar.Entities;
using poshtar.Services;

namespace poshtar.Smtp;

public class SessionContext : IDisposable
{
    public Guid ConnectionId { get; set; } = Guid.NewGuid();
    public Transaction Transaction { get; private set; }
    public bool IsSubmissionPort { get; }
    public bool CanRelay;
    public IServiceScope ServiceScope { get; }
    public AppDbContext Db { get; }
    public IpService IpSvc { get; }
    public EndpointDefinition EndpointDefinition { get; }
    public IPEndPoint? RemoteEndpoint { get; set; }
    public SecurableDuplexPipe? Pipe { get; set; }
    public bool IsAuthenticated => Transaction.FromUser != null;
    public bool IsQuitRequested { get; set; }
    // ANTI SPAM
    public int ConsecutiveCmdFail { get; set; }
    public int ConsecutiveRcptFail { get; set; }
    public SpfQualifier Spf { get; set; } = SpfQualifier.None;

    public SessionContext(IServiceProvider serviceProvider, EndpointDefinition endpointDefinition)
    {
        ServiceScope = serviceProvider.CreateScope();
        EndpointDefinition = endpointDefinition;
        IsSubmissionPort = endpointDefinition.Endpoint.Port == C.Smtp.EXPLICIT_SUBMISSION_PORT ||
                           endpointDefinition.Endpoint.Port == C.Smtp.IMPLICIT_SUBMISSION_PORT;

        Db = ServiceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Transaction = new() { ConnectionId = ConnectionId, Submission = IsSubmissionPort, Start = DateTime.MaxValue };
        Db.Transactions.Add(Transaction);
        IpSvc = ServiceScope.ServiceProvider.GetRequiredService<IpService>();
    }
    public void ResetTransaction()
    {
        FinishCurrentTransaction();
        Transaction = new()
        {
            ConnectionId = ConnectionId,
            Submission = Transaction.Submission,
            IpAddress = Transaction.IpAddress,
            CountryCode = Transaction.CountryCode,
            CountryName = Transaction.CountryName,
            Asn = Transaction.Asn,
            Start = DateTime.UtcNow,
            Client = Transaction.Client,
            FromUser = Transaction.FromUser,
            FromUserId = Transaction.FromUserId,
            Secure = Transaction.Secure,
        };
        Db.Transactions.Add(Transaction);
    }
    void FinishCurrentTransaction()
    {
        Transaction.End = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(Transaction.IpAddress) && RemoteEndpoint != null)
            Transaction.IpAddress = RemoteEndpoint.Address?.ToString();
    }
    public void Log(string message, object? properties = null) => Transaction.Logs.Add(new(message, properties));
    public void Dispose()
    {
        Pipe?.Dispose();
        if (Db != null)
        {
            FinishCurrentTransaction();
            if (Db.ChangeTracker.HasChanges())
                Db.SaveChanges();
            Db.Dispose();
        }
        ServiceScope?.Dispose();
    }
}