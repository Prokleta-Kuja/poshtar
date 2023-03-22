using Microsoft.EntityFrameworkCore;

namespace poshtar.Smtp.Commands;
public enum MailboxFilterResult { Yes = 0, NoTemporarily = 1, NoPermanently = 2, SizeLimitExceeded = 3 }
public class MailCommand : Command
{
    public const string Command = "MAIL";

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="parameters">The list of extended (ESMTP) parameters.</param>
    public MailCommand(EmailAddress address, IReadOnlyDictionary<string, string> parameters) : base(Command)
    {
        Address = address;
        Parameters = parameters;
    }

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

        if (ctx.EndpointDefinition.AuthenticationRequired && ctx.IsAuthenticated == false)
        {
            await ctx.Pipe.Output.WriteReplyAsync(Response.AuthenticationRequired, cancellationToken).ConfigureAwait(false);
            return false;
        }

        ctx.Transaction.Reset();
        ctx.Transaction.Parameters = Parameters;

        // check if a size has been defined
        var size = GetMessageSize();

        // check against the server supplied maximum
        if (C.MaxMessageSize > 0 && size > C.MaxMessageSize)
        {
            await ctx.Pipe.Output.WriteReplyAsync(Response.SizeLimitExceeded, cancellationToken).ConfigureAwait(false);
            return false;
        }

        ctx.Transaction.From = Address;
        if (ctx.IsSubmissionPort)
        {
            if (ctx.User == null)
            {
                ctx.Log("Cannot send without authentication", Address);
                await ctx.Pipe.Output.WriteReplyAsync(Response.MailboxNameNotAllowed, cancellationToken).ConfigureAwait(false);
                return false;
            }
            /*"SELECT u.name FROM addresses a 
                JOIN domains d USING(domain_id) 
                JOIN address_user au ON au.addresses_address_id = a.address_id 
                JOIN users u ON u.user_id = au.users_user_id 
                    WHERE d.disabled IS NULL 
                        AND a.disabled IS NULL 
                        AND u.disabled IS NULL 
                        AND d.name = '%d' 
                        AND ('%u' LIKE a.expression OR a.expression IS NULL)"*/

            System.Diagnostics.Debugger.Break();
            var canSend = await ctx.Db.Addresses
                .AsNoTracking()
                .Where(a => !a.Disabled.HasValue && (a.Expression == null || EF.Functions.Like(Address.User, a.Expression)))
                .Where(a => a.Users.Where(u => !u.Disabled.HasValue).Contains(ctx.User))
                .Where(a => a.Domain!.Name.Equals(Address.Host.ToLower()) && !a.Domain.Disabled.HasValue)
                .AnyAsync(cancellationToken).ConfigureAwait(false);
            // var domain = await ctx.Db.Domains
            //     .AsNoTracking()
            //     .Where(d => d.Name.Equals(Address.Host.ToLower()) && !d.Disabled.HasValue)
            //     .Include(d => d.Addresses.Where(a => a.Users.Contains(ctx.User) && !a.Disabled.HasValue))
            //     .SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            if (!canSend)
            {
                ctx.Log("Sending as address not allowed", Address);
                await ctx.Pipe.Output.WriteReplyAsync(Response.MailboxNameNotAllowed, cancellationToken).ConfigureAwait(false);
                return false;
            }

            // TODO: does user has enough space to accept message?
        }
        else
        {
            // TODO: check SPF
        }

        await ctx.Pipe.Output.WriteReplyAsync(Response.Ok, cancellationToken).ConfigureAwait(false);
        return true;
        // switch (await context.CanAcceptFromAsync(Address, size, cancellationToken).ConfigureAwait(false))
        // {
        //     case MailboxFilterResult.Yes:
        //         context.Transaction.From = Address;
        //         await context.Pipe.Output.WriteReplyAsync(Response.Ok, cancellationToken).ConfigureAwait(false);
        //         return true;

        //     case MailboxFilterResult.NoTemporarily:
        //         await context.Pipe.Output.WriteReplyAsync(Response.MailboxUnavailable, cancellationToken).ConfigureAwait(false);
        //         return false;

        //     case MailboxFilterResult.NoPermanently:
        //         await context.Pipe.Output.WriteReplyAsync(Response.MailboxNameNotAllowed, cancellationToken).ConfigureAwait(false);
        //         return false;

        //     case MailboxFilterResult.SizeLimitExceeded:
        //         await context.Pipe.Output.WriteReplyAsync(Response.SizeLimitExceeded, cancellationToken).ConfigureAwait(false);
        //         return false;
        // }

        //throw new ResponseException(Response.TransactionFailed);
    }

    /// <summary>
    /// Gets the estimated message size supplied from the ESMTP command extension.
    /// </summary>
    /// <returns>The estimated message size that was supplied by the client.</returns>
    int GetMessageSize()
    {
        if (Parameters.TryGetValue("SIZE", out var value) == false)
            return 0;

        return int.TryParse(value, out var size) == false ? 0 : size;
    }

    /// <summary>
    /// Gets the address that the mail is from.
    /// </summary>
    public EmailAddress Address { get; }

    /// <summary>
    /// The list of extended mail parameters.
    /// </summary>
    public IReadOnlyDictionary<string, string> Parameters { get; }
}