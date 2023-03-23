using Hangfire;

namespace poshtar.Jobs;

[Queue(C.Hangfire.Queue.Out)]
public class OutgoingJob
{
    public void Deliver(Guid sessionId)
    {

    }
}