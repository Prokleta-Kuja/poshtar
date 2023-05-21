using System.Net;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using ARSoft.Tools.Net.Spf;

namespace poshtar.Smtp;

public static class AntiSpam
{
    public static async Task CheckRblAsync(this SessionContext ctx)
    {
        if (ctx.Transaction.Secure || C.IsDebug)
            return;
        if (ctx.Transaction.IpAddress == null)
        {
            ctx.Log("No IP specified, closing connection");
            throw new ResponseException(new Response(ReplyCode.TransactionFailed, "Invalid IP"), true);
        }

        var revIp = BlockList.ReverseIp(ctx.Transaction.IpAddress);

        var checks = await Task.WhenAll(BlockList.DefaultLists.Select(l => l.CheckAsync(revIp)));

        (int listed, int notListed, int failed) result = new();
        foreach (var check in checks)
            if (!check.Listed.HasValue)
                result.failed++;
            else if (check.Listed.Value)
                result.listed++;
            else
                result.notListed++;

        ctx.Log($"RBL listed: {result.listed}, not listed: {result.notListed}, failed: {result.failed}", checks);
    }
    public static void CmdAccepted(this SessionContext ctx) => ctx.ConsecutiveCmdFail = 0;
    public static void CmdTry(this SessionContext ctx)
    {
        ctx.ConsecutiveCmdFail++;
        if (ctx.ConsecutiveCmdFail > C.Smtp.AntiSpam.ConsecutiveCmdFail)
        {
            ctx.Log("Command failed count over treshold, closing connection");
            throw new ResponseException(new Response(ReplyCode.TransactionFailed), true);
        }
    }
    public static async Task CheckHeloEhloAsync(this SessionContext ctx)
    {
        if (ctx.Transaction.Secure || C.IsDebug)
            return;
        if (ctx.Transaction.IpAddress == null)
        {
            ctx.Log("No IP specified, closing connection");
            throw new ResponseException(new Response(ReplyCode.TransactionFailed, "Invalid IP"), true);
        }
        if (string.IsNullOrWhiteSpace(ctx.Transaction.Client))
        {
            ctx.Log("No HELO/EHLO specified, closing connection");
            throw new ResponseException(new Response(ReplyCode.TransactionFailed, "Invalid HELO/EHLO"), true);
        }

        bool? forwardSuccess = null;
        bool? reverseSuccess = null;
        if (IPAddress.TryParse(ctx.Transaction.IpAddress, out var ip) && DomainName.TryParse(ctx.Transaction.Client, out var domain))
        {
            var fwdTask = DnsClient.Default.ResolveAsync(domain!);
            var rvsTask = DnsClient.Default.ResolveAsync(ip.GetReverseLookupDomain(), RecordType.Ptr);
            await Task.WhenAll(fwdTask, rvsTask);
            var fwdMessage = fwdTask.Result;
            var rvsMessage = rvsTask.Result;

            if (fwdMessage == null || (fwdMessage.ReturnCode != ReturnCode.NoError && fwdMessage.ReturnCode != ReturnCode.NxDomain))
                forwardSuccess = null;
            else if (fwdMessage.AnswerRecords.Count == 0)
                forwardSuccess = false;
            else
                foreach (DnsRecordBase dnsRecord in fwdMessage.AnswerRecords)
                    if (dnsRecord is ARecord aRecord)
                    {
                        forwardSuccess = aRecord.Address.Equals(ip);
                        if (forwardSuccess.Value)
                            break;
                    }

            if (rvsMessage == null || (rvsMessage.ReturnCode != ReturnCode.NoError && rvsMessage.ReturnCode != ReturnCode.NxDomain))
                reverseSuccess = null;
            else if (rvsMessage.AnswerRecords.Count == 0)
                reverseSuccess = false;
            else
                foreach (DnsRecordBase dnsRecord in rvsMessage.AnswerRecords)
                    if (dnsRecord is PtrRecord ptrRecord)
                    {
                        reverseSuccess = ptrRecord.PointerDomainName == domain;
                        if (reverseSuccess.Value)
                            break;
                    }
        }
        if (!forwardSuccess.HasValue)
            ctx.Log("Forward DNS lookup failed");
        else if (forwardSuccess.Value)
            ctx.Log("Forward DNS lookup matches IP");
        else
        {
            ctx.Log("No forward DNS matches, closing connection");
            throw new ResponseException(new Response(ReplyCode.TransactionFailed, "Forward DNS lookup didn't match the IP"), true);
        }
        if (!reverseSuccess.HasValue)
            ctx.Log("Reverse DNS lookup failed");
        else if (reverseSuccess.Value)
            ctx.Log("Reverse DNS lookup matches HELO/EHLO");
        else
        {
            ctx.Log("No reverse DNS matches, closing connection");
            throw new ResponseException(new Response(ReplyCode.TransactionFailed, "Reverse DNS lookup didn't match the HELO/EHLO"), true);
        }
    }
    public static async Task CheckSpfAsync(this SessionContext ctx, string senderDomain)
    {
        if (C.IsDebug)
            return;
        if (ctx.Transaction.IpAddress == null)
        {
            ctx.Log("No IP specified, closing connection");
            throw new ResponseException(new Response(ReplyCode.TransactionFailed, "Invalid IP"), true);
        }
        if (string.IsNullOrWhiteSpace(senderDomain))
        {
            ctx.Log("Invalid MAIL FROM, closing connection");
            throw new ResponseException(new Response(ReplyCode.TransactionFailed, "Invalid sender"), true);
        }
        if (IPAddress.TryParse(ctx.Transaction.IpAddress, out var ip) && DomainName.TryParse(senderDomain, out var domain))
        {
            var validator = new SpfValidator();
            var result = await validator.CheckHostAsync(ip, domain!, string.Empty);
            ctx.Spf = result.Result;
        }
        switch (ctx.Spf)
        {
            case SpfQualifier.Pass:
                ctx.Log($"SPF {ctx.Spf}");
                break;
            case SpfQualifier.Fail:
            case SpfQualifier.SoftFail:  //SPF softfail is interpreted in DMARC as fail by default
            case SpfQualifier.None:  //SPF none is treated as fail in DMARC
            case SpfQualifier.Neutral:  //SPF neutral is interpreted in DMARC as fail by default
            case SpfQualifier.TempError:  //the error is used to return a 4xx status code and the SMTP session ends
            case SpfQualifier.PermError:  //SPF permerror is interpreted in DMARC as fail
                ctx.Log($"SPF {ctx.Spf}. Closing connection.");
                throw new ResponseException(new Response(ReplyCode.TransactionFailed, $"SPF {ctx.Spf}"), true);
        }
    }
    public static void LocalRecipientResolved(this SessionContext ctx) => ctx.ConsecutiveRcptFail = 0;
    public static void LocalRecipientNotResolved(this SessionContext ctx)
    {
        ctx.ConsecutiveRcptFail++;
        if (ctx.ConsecutiveRcptFail >= C.Smtp.AntiSpam.ConsecutiveRcptFail)
        {
            ctx.Log("Consecutive RCPT command failed count over treshold, closing connection");
            throw new ResponseException(new Response(ReplyCode.TransactionFailed), true);
        }
    }
}
public class BlockList
{
    public string Zone { get; set; }
    public BlockList(string zone)
    {
        Zone = zone;
    }
    public static string ReverseIp(string ip) => string.Join('.', ip.Split('.').Reverse());
    public async Task<BlockListResult> CheckAsync(string reversedIp)
    {
        var q = $"{reversedIp}.{Zone}";
        var result = new BlockListResult { Zone = Zone };

        if (!DomainName.TryParse(q, out var query))
            return result;

        var dnsMessage = await DnsClient.Default.ResolveAsync(query!);
        if ((dnsMessage == null) || ((dnsMessage.ReturnCode != ReturnCode.NoError) && (dnsMessage.ReturnCode != ReturnCode.NxDomain)))
        { }
        else if (dnsMessage.AnswerRecords.Count == 0)
            result.Listed = false;
        else if (dnsMessage.AnswerRecords.Count == 1 && dnsMessage.AnswerRecords[0] is ARecord aRecord && aRecord.Address.ToString().StartsWith("127.0.0."))
            result.Listed = true;

        return result;
    }
    public static List<BlockList> DefaultLists { get; set; } = new List<BlockList>
    {
        new ("bl.mxrbl.com"),
        new ("b.barracudacentral.org"),
        new ("dnsbl.justspam.org"),
        //new ("zen.spamhaus.org"),
    };
}
public class BlockListResult
{
    public required string Zone { get; set; }
    public bool? Listed { get; set; }
}