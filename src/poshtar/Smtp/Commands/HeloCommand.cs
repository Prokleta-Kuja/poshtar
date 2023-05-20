using System.Net;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;

namespace poshtar.Smtp.Commands;

public class HeloCommand : Command
{
    public const string Command = "HELO";

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="domainOrAddress">The domain name.</param>
    public HeloCommand(string domainOrAddress) : base(Command)
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

        ctx.Log($"HELO {DomainOrAddress}");
        ctx.Transaction.Client = DomainOrAddress;

        bool? success = null;
        if (IPAddress.TryParse(ctx.Transaction.IpAddress, out var ip) && DomainName.TryParse(DomainOrAddress, out var domain))
        {
            var dnsMessage = DnsClient.Default.Resolve(IPAddress.Parse(ctx.Transaction.IpAddress).GetReverseLookupDomain(), RecordType.Ptr);
            if ((dnsMessage == null) || ((dnsMessage.ReturnCode != ReturnCode.NoError) && (dnsMessage.ReturnCode != ReturnCode.NxDomain)))
                ctx.Log("PTR DNS request failed");
            else if (dnsMessage.AnswerRecords.Count == 0)
            {
                success = false;
                ctx.Log("No PTR records found. Closing connection.");
            }
            else
                foreach (DnsRecordBase dnsRecord in dnsMessage.AnswerRecords)
                    if (dnsRecord is PtrRecord ptrRecord)
                    {
                        success = ptrRecord.PointerDomainName == domain;
                        if (success.Value)
                            ctx.Log("PTR matches HELO/EHLO");
                        else
                            ctx.Log($"PTR {ptrRecord.PointerDomainName} does not match HELO/EHLO. Closing connection.");
                    }
        }
        if (success.HasValue && !success.Value)
            throw new ResponseException(Response.ServiceClosingTransmissionChannel, true);

        var response = new Response(ReplyCode.Ok, GetGreeting(ctx));
        await ctx.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);
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
    /// Gets the domain name.
    /// </summary>
    public string DomainOrAddress { get; }
}