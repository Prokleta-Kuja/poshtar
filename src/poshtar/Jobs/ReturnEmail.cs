using Hangfire.Server;

namespace poshtar.Jobs;

public class ReturnEmail
{
    public async Task Run(Guid sessionId, PerformContext context, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}