using System.Net;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;

namespace poshtar.Smtp.Commands;

public class EhloCommand : Command
{
    public const string Command = "EHLO";

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="domainOrAddress">The domain name or address literal.</param>
    public EhloCommand(string domainOrAddress) : base(Command)
    {
        DomainOrAddress = domainOrAddress;
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

        ctx.Log($"EHLO {DomainOrAddress}");
        ctx.Transaction.Client = DomainOrAddress;

        bool? success = null;
        if (IPAddress.TryParse(ctx.Transaction.IpAddress, out var ip) && DomainName.TryParse(DomainOrAddress, out var domain))
        {
            var dnsMessage = DnsClient.Default.Resolve(ip.GetReverseLookupDomain(), RecordType.Ptr);
            if ((dnsMessage == null) || ((dnsMessage.ReturnCode != ReturnCode.NoError) && (dnsMessage.ReturnCode != ReturnCode.NxDomain)))
                success = null;
            else if (dnsMessage.AnswerRecords.Count == 0)
                success = false;
            else
                foreach (DnsRecordBase dnsRecord in dnsMessage.AnswerRecords)
                    if (dnsRecord is PtrRecord ptrRecord)
                    {
                        success = ptrRecord.PointerDomainName == domain;
                        if (success.Value)
                            break;
                    }
        }
        if (!success.HasValue)
            ctx.Log("PTR lookup failed");
        else if (success.Value)
            ctx.Log("PTR matches HELO/EHLO");
        else
        {
            ctx.Log("No matching PTR record found. Closing connection.");
            throw new ResponseException(Response.ServiceClosingTransmissionChannel, true);
        }

        var output = new[] { GetGreeting(ctx) }.Union(GetExtensions(ctx)).ToArray();

        for (var i = 0; i < output.Length - 1; i++)
            ctx.Pipe.Output.WriteLine($"250-{output[i]}");

        ctx.Pipe.Output.WriteLine($"250 {output[^1]}");

        await ctx.Pipe.Output.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Returns the greeting to display to the remote host.
    /// </summary>
    /// <param name="ctx">The session context.</param>
    /// <returns>The greeting text to display to the remote host.</returns>
    protected virtual string GetGreeting(SessionContext ctx)
    {
        if (ctx.IsSubmissionPort)
            return $"{C.Hostname} Hello {DomainOrAddress}, what do you want to send today?";
        else
            return $"{C.Hostname} Hello {DomainOrAddress}, got any emails for me?";
    }

    /// <summary>
    /// Returns the list of extensions that are current for the context.
    /// </summary>
    /// <param name="ctx">The session context.</param>
    /// <returns>The list of extensions that are current for the context.</returns>
    protected virtual IEnumerable<string> GetExtensions(SessionContext ctx)
    {
        yield return "PIPELINING";
        yield return "8BITMIME";
        yield return "SMTPUTF8";
        yield return "STARTTLS";

        if (C.MaxMessageSize > 0)
            yield return $"SIZE {C.MaxMessageSize}";

        if (ctx.IsSubmissionPort)
            yield return "AUTH PLAIN LOGIN";
    }

    /// <summary>
    /// Gets the domain name or address literal.
    /// </summary>
    public string DomainOrAddress { get; }
}