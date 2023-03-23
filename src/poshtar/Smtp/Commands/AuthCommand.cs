using System.IO.Pipelines;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using poshtar.Services;

namespace poshtar.Smtp.Commands;

public enum AuthenticationMethod { Login, Plain }

public class AuthCommand : Command
{
    public const string Command = "AUTH";
    const string BASE64_USERNAME = "VXNlcm5hbWU6"; // 'Username' encoded in base64
    const string BASE64_PASSWORD = "UGFzc3dvcmQ6"; // 'Password' encoded in base64
    const string FAIL_NOT_PARSABLE = "Invalid authentication attempt";
    const string FAIL_INVALID_PARAMS = "Authentication failed, invalid username and/or password provided";
    string? _user;
    string? _password;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="method">The authentication method.</param>
    /// <param name="parameter">The authentication parameter.</param>
    public AuthCommand(AuthenticationMethod method, string? parameter) : base(Command)
    {
        Method = method;
        Parameter = parameter ?? string.Empty;
    }

    /// <summary>
    /// Execute the command.
    /// </summary>
    /// <param name="ctx">The execution context to operate on.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns true if the command executed successfully such that the transition to the next state should occur, false 
    /// if the current state is to be maintained.</returns>
    internal override async Task<bool> ExecuteAsync(SessionContext ctx, CancellationToken cancellationToken)
    {
        if (!ctx.IsSubmissionPort)
        {
            ctx.Log("Authentication not allowed");
            return false;
        }
        if (ctx.RemoteEndpoint == null || ctx.Pipe == null)
            return false;
        if (!ctx.Pipe.IsSecure)
        {
            ctx.Log("Unsecure authentication not allowed");
            return false;
        }

        switch (Method)
        {
            case AuthenticationMethod.Plain: // both login and password in the same Base64-encoded string
                if (await TryPlainAsync(ctx, cancellationToken).ConfigureAwait(false) == false)
                    return await FailResponse(ctx.Pipe, FAIL_NOT_PARSABLE, cancellationToken).ConfigureAwait(false);
                ctx.Log("AUTH PLAIN");
                break;

            case AuthenticationMethod.Login: // login and password separately
                if (await TryLoginAsync(ctx, cancellationToken).ConfigureAwait(false) == false)
                    return await FailResponse(ctx.Pipe, FAIL_NOT_PARSABLE, cancellationToken).ConfigureAwait(false);
                ctx.Log("AUTH LOGIN");
                break;
        }

        if (string.IsNullOrWhiteSpace(_user) || string.IsNullOrWhiteSpace(_password))
        {
            ctx.Log("Auth failed, no username or password provided");
            return await FailResponse(ctx.Pipe, FAIL_INVALID_PARAMS, cancellationToken).ConfigureAwait(false);
        }

        var userLower = _user.ToLower();
        var dbUser = await ctx.Db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Name.Equals(userLower) && !u.Disabled.HasValue, cancellationToken).ConfigureAwait(false);
        if (dbUser == null)
        {
            ctx.Log($"Authentication failed, invalid user", new { _user });
            return await FailResponse(ctx.Pipe, FAIL_INVALID_PARAMS, cancellationToken).ConfigureAwait(false);
        }

        if (!DovecotHasher.Verify(dbUser.Salt, dbUser.Hash, _password))
        {
            ctx.Log($"Authentication failed, invalid password", new { _user });
            return await FailResponse(ctx.Pipe, FAIL_INVALID_PARAMS, cancellationToken).ConfigureAwait(false);
        }

        ctx.User = dbUser;
        ctx.Log($"Authenticated as {_user}");
        await ctx.Pipe.Output.WriteReplyAsync(Response.AuthenticationSuccessful, cancellationToken).ConfigureAwait(false);
        return true;
    }

    static async Task<bool> FailResponse(SecurableDuplexPipe pipe, string message, CancellationToken cancellationToken)
    {
        var response = new Response(ReplyCode.AuthenticationFailed, message);
        await pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);

        // if (should close connection)
        //     throw new ResponseException(Response.ServiceClosingTransmissionChannel, true);

        return false;
    }

    /// <summary>
    /// Attempt a PLAIN login sequence.
    /// </summary>
    /// <param name="context">The execution context to operate on.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>true if the PLAIN login sequence worked, false if not.</returns>
    async Task<bool> TryPlainAsync(SessionContext context, CancellationToken cancellationToken)
    {
        var authentication = Parameter;

        if (string.IsNullOrWhiteSpace(authentication) && context.Pipe != null)
        {
            await context.Pipe.Output.WriteReplyAsync(new Response(ReplyCode.ContinueWithAuth, " "), cancellationToken).ConfigureAwait(false);
            authentication = await context.Pipe.Input.ReadLineAsync(Encoding.ASCII, cancellationToken).ConfigureAwait(false);
        }

        if (TryExtractFromBase64(authentication) == false)
        {
            if (context.Pipe != null)
                await context.Pipe.Output.WriteReplyAsync(Response.AuthenticationFailed, cancellationToken).ConfigureAwait(false);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Attempt to extract the user name and password combination from a single line base64 encoded string.
    /// </summary>
    /// <param name="base64">The base64 encoded string to extract the user name and password from.</param>
    /// <returns>true if the user name and password were extracted from the base64 encoded string, false if not.</returns>
    bool TryExtractFromBase64(string base64)
    {
        var match = Regex.Match(Encoding.UTF8.GetString(Convert.FromBase64String(base64)), "\x0000(?<user>.*)\x0000(?<password>.*)");
        if (!match.Success)
            return false;

        _user = match.Groups["user"].Value;
        _password = match.Groups["password"].Value;

        return true;
    }

    /// <summary>
    /// Attempt a LOGIN login sequence.
    /// </summary>
    /// <param name="context">The execution context to operate on.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>true if the LOGIN login sequence worked, false if not.</returns>
    async Task<bool> TryLoginAsync(SessionContext context, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(Parameter))
            _user = Encoding.UTF8.GetString(Convert.FromBase64String(Parameter));
        else
        {
            if (context.Pipe != null)
            {
                await context.Pipe.Output.WriteReplyAsync(new(ReplyCode.ContinueWithAuth, BASE64_USERNAME), cancellationToken).ConfigureAwait(false);
                _user = await ReadBase64EncodedLineAsync(context.Pipe.Input, cancellationToken).ConfigureAwait(false);
            }
        }

        if (context.Pipe != null)
        {
            await context.Pipe.Output.WriteReplyAsync(new(ReplyCode.ContinueWithAuth, BASE64_PASSWORD), cancellationToken).ConfigureAwait(false);
            _password = await ReadBase64EncodedLineAsync(context.Pipe.Input, cancellationToken).ConfigureAwait(false);
        }

        return true;
    }

    /// <summary>
    /// Read a Base64 encoded line.
    /// </summary>
    /// <param name="reader">The pipe to read from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The decoded Base64 string.</returns>
    static async Task<string> ReadBase64EncodedLineAsync(PipeReader reader, CancellationToken cancellationToken)
    {
        var text = await reader.ReadLineAsync(cancellationToken);
        return text == null ? string.Empty : Encoding.UTF8.GetString(Convert.FromBase64String(text));
    }

    /// <summary>
    /// The authentication method.
    /// </summary>
    public AuthenticationMethod Method { get; }

    /// <summary>
    /// The authentication parameter.
    /// </summary>
    public string Parameter { get; }
}