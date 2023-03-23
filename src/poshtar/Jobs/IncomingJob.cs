using Hangfire;

namespace poshtar.Jobs;

[Queue(C.Hangfire.Queue.In)]
public class IncomingJob
{
    public void Deliver(Guid sessionId)
    {

    }
}