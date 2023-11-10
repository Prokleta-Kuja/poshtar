using System.Buffers;
using System.IO.Pipelines;
using poshtar.Smtp.Commands;

namespace poshtar.Smtp;

class Session
{
    static readonly TimeSpan s_commandTimeOut = TimeSpan.FromMinutes(5);
    readonly StateMachine _stateMachine;
    readonly SessionContext _context;
    readonly CommandFactory _commandFactory;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="context">The session context.</param>
    internal Session(SessionContext context)
    {
        _context = context;
        _stateMachine = new StateMachine(_context);
        _commandFactory = new();
    }

    /// <summary>
    /// Handles the SMTP session.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task which performs the operation.</returns>
    internal async Task RunAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        await OutputGreetingAsync(cancellationToken).ConfigureAwait(false);
        await ExecuteAsync(_context, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Execute the command handler against the specified session context.
    /// </summary>
    /// <param name="ctx">The session context to execute the command handler against.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task which asynchronously performs the execution.</returns>
    async Task ExecuteAsync(SessionContext ctx, CancellationToken cancellationToken)
    {
        while (ctx.IsQuitRequested == false && cancellationToken.IsCancellationRequested == false)
        {
            try
            {
                await AntiSpam.CmdTryAsync(ctx);
                var command = await ReadCommandAsync(ctx, cancellationToken).ConfigureAwait(false);

                if (command == null)
                    return;

                if (_stateMachine.TryAccept(command, out var errorResponse) == false)
                    throw new ResponseException(errorResponse!);

                if (await ExecuteAsync(command, ctx, cancellationToken).ConfigureAwait(false))
                    _stateMachine.Transition(ctx);

                AntiSpam.CmdAccepted(ctx);
            }
            catch (ResponseException responseException) when (responseException.IsQuitRequested)
            {
                if (ctx.Pipe != null)
                    await ctx.Pipe.Output.WriteReplyAsync(responseException.Response, cancellationToken).ConfigureAwait(false);

                ctx.IsQuitRequested = true;
            }
            catch (ResponseException responseException)
            {
                var response = CreateErrorResponse(responseException.Response, ctx.ConsecutiveCmdFail);

                if (ctx.Pipe != null)
                    await ctx.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                if (ctx.Pipe != null)
                    await ctx.Pipe.Output.WriteReplyAsync(new Response(ReplyCode.ServiceClosingTransmissionChannel, "The session has be cancelled."), CancellationToken.None).ConfigureAwait(false);
            }
        }
    }

    async ValueTask<Command> ReadCommandAsync(SessionContext context, CancellationToken cancellationToken)
    {
        var timeout = new CancellationTokenSource(s_commandTimeOut);
        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);

        try
        {
            Command? command = null;

            if (context.Pipe != null)
                await context.Pipe.Input.ReadLineAsync(
                    buffer =>
                    {
                        var parser = new Parser(_commandFactory);
                        if (parser.TryMake(ref buffer, out command, out var errorResponse) == false)
                        {
                            try
                            {
                                var bytes = buffer.ToArray();
                                var text = System.Text.Encoding.ASCII.GetString(bytes);
                                context.Log($"Unrecognized command: {text}");
                            }
                            catch (Exception) { }
                            finally
                            {
                                throw new ResponseException(errorResponse!);
                            }
                        }

                        return Task.CompletedTask;
                    },
                    cancellationTokenSource.Token).ConfigureAwait(false);

            return command!;
        }
        catch (OperationCanceledException)
        {
            if (timeout.IsCancellationRequested)
                throw new ResponseException(new Response(ReplyCode.ServiceClosingTransmissionChannel, "Timeout whilst waiting for input."), true);

            throw new ResponseException(new Response(ReplyCode.ServiceClosingTransmissionChannel, "The session has be cancelled."), true);
        }
        finally
        {
            timeout.Dispose();
            cancellationTokenSource.Dispose();
        }
    }

    /// <summary>
    /// Create an error response.
    /// </summary>
    /// <param name="response">The original response to wrap with the error message information.</param>
    /// <param name="retries">The number of retries remaining before the session is terminated.</param>
    /// <returns>The response that wraps the original response with the additional error information.</returns>
    static Response CreateErrorResponse(Response response, int failCount)
    {
        return new Response(response.ReplyCode, $"{response.Message}, fail count: {failCount}");
    }

    /// <summary>
    /// Execute the command.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="context">The execution context to operate on.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task which asynchronously performs the execution.</returns>
    static async Task<bool> ExecuteAsync(Command command, SessionContext context, CancellationToken cancellationToken)
    {
        var result = await command.ExecuteAsync(context, cancellationToken);
        return result;
    }

    /// <summary>
    /// Output the greeting.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task which performs the operation.</returns>
    ValueTask<FlushResult> OutputGreetingAsync(CancellationToken cancellationToken)
    {
        _context.Pipe?.Output.WriteLine($"220 {C.Hostname} ESMTP {_context.ConnectionId}");
        return _context.Pipe!.Output.FlushAsync(cancellationToken);
    }
}