using System.Buffers;
using Hangfire;
using poshtar.Jobs;

namespace poshtar.Smtp.Commands;

public class DataCommand : Command
{
    public const string Command = "DATA";

    /// <summary>
    /// Constructor.
    /// </summary>
    public DataCommand() : base(Command) { }

    /// <summary>
    /// Execute the command.
    /// </summary>
    /// <param name="ctx">The execution context to operate on.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
    /// if the current state is to be maintained.</returns>
    internal override async Task<bool> ExecuteAsync(SessionContext ctx, CancellationToken cancellationToken)
    {
        if (ctx.Pipe == null)
            return false;

        ctx.Log($"DATA requested");
        if (ctx.Transaction.InternalUsers.Count == 0 && ctx.Transaction.ExternalAddresses.Count == 0)
        {
            ctx.Log($"Refuse, no recepients given");
            await ctx.Pipe.Output.WriteReplyAsync(Response.NoValidRecipientsGiven, cancellationToken).ConfigureAwait(false);
            return false;
        }

        await ctx.Pipe.Output.WriteReplyAsync(new Response(ReplyCode.StartMailInput, "Go ahead with the message and end it with <CRLF>.<CRLF>"), cancellationToken).ConfigureAwait(false);

        try
        {
            Response? response = null;
            await ctx.Pipe.Input.ReadDotBlockAsync(
                async buffer =>
                {
                    response = await SaveAsync(ctx, buffer, cancellationToken).ConfigureAwait(false);
                },
                cancellationToken).ConfigureAwait(false);

            if (response != null)
                await ctx.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            ctx.Log($"Failed to receive content");
            await ctx.Pipe.Output.WriteReplyAsync(new Response(ReplyCode.TransactionFailed), cancellationToken).ConfigureAwait(false);
        }

        return true;
    }
    static async Task<Response> SaveAsync(SessionContext ctx, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        await ctx.Db.SaveChangesAsync(cancellationToken); // Must get TransactionId before using it for file name
        var emlPath = C.Paths.QueueDataFor($"{ctx.Transaction.TransactionId}.eml");
        try
        {
            await using var emlStream = File.Create(emlPath);
            var position = buffer.GetPosition(0);
            while (buffer.TryGet(ref position, out var memory))
                await emlStream.WriteAsync(memory, cancellationToken);

            foreach (var internalUser in ctx.Transaction.InternalUsers)
                ctx.Transaction.Recipients.Add(new(internalUser.Key, internalUser.Value));
            if (ctx.Transaction.ExternalAddresses.Count > 0)
                ctx.Transaction.Recipients.Add(new(ctx.Transaction.ExternalAddresses));

            ctx.Log("Email scheduled for delivery");
            await ctx.Db.SaveChangesAsync(cancellationToken);

            var jobClient = ctx.ServiceScope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
            jobClient.Enqueue<DeliverEmail>(i => i.Run(ctx.Transaction.TransactionId, null!, CancellationToken.None));
        }
        catch (Exception)
        {
            File.Delete(emlPath);
            throw;
        }

        return Response.Ok;
    }
}