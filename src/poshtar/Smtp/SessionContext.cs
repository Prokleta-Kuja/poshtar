using System.Buffers;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using poshtar.Entities;

namespace poshtar.Smtp;

public class SessionContext : IDisposable
{
    public Guid ContextId { get; } = Guid.NewGuid();
    public bool IsSubmissionPort;
    public IServiceProvider ServiceProvider { get; }
    public AppDbContext Db { get; }
    public IMemoryCache Cache { get; }
    public ServerOptions ServerOptions { get; }
    public EndpointDefinition EndpointDefinition { get; }
    public IPEndPoint? RemoteEndpoint { get; set; }
    public SecurableDuplexPipe? Pipe { get; set; }
    public MessageTransaction Transaction { get; }
    public User? User { get; set; }
    public bool IsAuthenticated => User != null;
    public bool IsQuitRequested { get; set; }
    public Dictionary<string, object> Properties { get; }
    public SessionContext(IServiceProvider serviceProvider, ServerOptions options, EndpointDefinition endpointDefinition)
    {
        ServiceProvider = serviceProvider;
        ServerOptions = options;
        EndpointDefinition = endpointDefinition;
        IsSubmissionPort = endpointDefinition.AuthenticationRequired;
        Transaction = new MessageTransaction();
        Properties = new();

        var dbFactory = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        Db = dbFactory.CreateDbContext();

        var cache = serviceProvider.GetRequiredService<IMemoryCache>();
        Cache = cache;
    }
    public void Log(string message, object? properties = null) => Db.Logs.Add(new(ContextId, message, properties));

    public async Task<bool> ShouldBlockConnectionFrom(IPEndPoint ip)
    {
        await Task.CompletedTask;
        if (IsSubmissionPort)
        {
            var failedAuthCountKey = C.CacheKeys.FailedAuthCountFor(ip);
            if (Cache.TryGetValue<int>(failedAuthCountKey, out var result))
                return result >= 8; // TODO: move to config
        }

        return false;
    }
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
    }
}