using System.Net;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using ARSoft.Tools.Net.Spf;
using Microsoft.Extensions.Caching.Memory;
using poshtar.Services;

namespace poshtar.Smtp;

public class AntiSpamSettings
{
    public int BanMinutes { get; set; } = 60;
    public int TarpitSeconds { get; set; } = 60;
    public bool EnableFCrDNSWorkarounds { get; set; }

    public int ConsecutiveCmdFail { get; set; } = 5;
    public bool BanConsecutiveCmdFail { get; set; }
    public bool TarpitConsecutiveCmdFail { get; set; }

    public int ConsecutiveRcptFail { get; set; } = 3;
    public bool BanConsecutiveRcptFail { get; set; }
    public bool TarpitConsecutiveRcptFail { get; set; }

    public string[] AsnBlacklist { get; set; } = Array.Empty<string>();
    public bool BanAsnBlacklist { get; set; }
    public bool TarpitAsnBlacklist { get; set; }

    public string[] ClientBlacklist { get; set; } = Array.Empty<string>();
    public bool BanClientBlacklist { get; set; }
    public bool TarpitClientBlacklist { get; set; }

    public bool EnforceForwardDns { get; set; }
    public bool BanForwardDns { get; set; }
    public bool TarpitForwardDns { get; set; }

    public bool EnforceReverseDns { get; set; }
    public bool BanReverseDns { get; set; }
    public bool TarpitReverseDns { get; set; }

    public bool EnforceDnsBlockList { get; set; }
    public bool BanDnsBlockList { get; set; }
    public bool TarpitDnsBlockList { get; set; }

    public bool EnforceSpf { get; set; }
    public bool BanSpf { get; set; }
    public bool TarpitSpf { get; set; }

}
public static class AntiSpam
{
    static string GetBannedIpKey(string ip) => $"ipban.{ip}";
    static void BanIp(this SessionContext ctx, string message)
    {
        if (string.IsNullOrWhiteSpace(ctx.Transaction.IpAddress) || C.Smtp.AntiSpamSettings.BanMinutes <= 0)
            return;

        var key = GetBannedIpKey(ctx.Transaction.IpAddress);
        var cache = ctx.ServiceScope.ServiceProvider.GetRequiredService<IMemoryCache>();
        cache.Set(key, message, TimeSpan.FromMinutes(C.Smtp.AntiSpamSettings.BanMinutes));
    }
    public static bool IsBannedIp(this SessionContext ctx)
    {
        if (string.IsNullOrWhiteSpace(ctx.Transaction.IpAddress))
            return false;

        var key = GetBannedIpKey(ctx.Transaction.IpAddress);
        var cache = ctx.ServiceScope.ServiceProvider.GetRequiredService<IMemoryCache>();
        if (cache.TryGetValue<string>(key, out var bannedReason))
        {
            ctx.Log($"{ctx.Transaction.IpAddress} banned {bannedReason}, terminating connection");
            return true;
        }

        return false;
    }
    static async Task TarpitAsync(this SessionContext ctx)
    {
        if (C.Smtp.AntiSpamSettings.TarpitSeconds <= 0)
            return;

        ctx.Log($"Tarpit for {C.Smtp.AntiSpamSettings.TarpitSeconds} seconds");
        await Task.Delay(TimeSpan.FromSeconds(C.Smtp.AntiSpamSettings.TarpitSeconds));
    }
    public static void CmdAccepted(this SessionContext ctx) => ctx.ConsecutiveCmdFail = 0;
    public static async Task CmdTryAsync(this SessionContext ctx)
    {
        if (C.Smtp.AntiSpamSettings.ConsecutiveCmdFail <= 0)
            return;

        if (C.Smtp.AntiSpamSettings.TarpitConsecutiveCmdFail)
        {
            var perTry = TimeSpan.FromSeconds(C.Smtp.AntiSpamSettings.TarpitSeconds)
                / C.Smtp.AntiSpamSettings.ConsecutiveCmdFail;
            var delay = perTry * ctx.ConsecutiveCmdFail;
            if (delay > TimeSpan.Zero)
            {
                ctx.Log($"Tarpit for {delay.TotalSeconds} seconds");
                await Task.Delay(delay);
            }
        }

        ctx.ConsecutiveCmdFail++;
        if (ctx.ConsecutiveCmdFail > C.Smtp.AntiSpamSettings.ConsecutiveCmdFail)
        {
            if (C.Smtp.AntiSpamSettings.BanConsecutiveCmdFail)
                BanIp(ctx, "Command failed count over treshold");
            ctx.Log("Command failed count over treshold, closing connection");
            throw new ResponseException(new Response(ReplyCode.TransactionFailed), true);
        }
    }
    public static async Task CheckAsnBlacklistAsync(this SessionContext ctx)
    {
        if (C.Smtp.AntiSpamSettings.AsnBlacklist.Length == 0 || ctx.Transaction.Asn is null)
            return;

        var result = false;
        var asnUp = ctx.Transaction.Asn.ToUpper();
        foreach (var asn in C.Smtp.AntiSpamSettings.AsnBlacklist)
            if (result)
                break;
            else if (asn.StartsWith('*'))
            {
                var asnEnd = asn.TrimStart('*').ToUpper();
                result = asnUp.EndsWith(asnEnd);
            }
            else if (asn.EndsWith('*'))
            {
                var asnStart = asn.TrimEnd('*').ToUpper();
                result = asnUp.StartsWith(asnStart);
            }
            else
                result = asnUp == asn.ToUpper();

        if (!result)
            return;

        ctx.Log("Matches ASN blacklist");
        if (C.Smtp.AntiSpamSettings.BanAsnBlacklist)
            BanIp(ctx, "ASN blacklist");
        if (C.Smtp.AntiSpamSettings.TarpitAsnBlacklist)
            await TarpitAsync(ctx);

        ctx.Log("Closing connection");
        throw new ResponseException(new Response(ReplyCode.TransactionFailed), true);
    }
    public static async Task CheckClientBlacklistAsync(this SessionContext ctx)
    {
        if (C.Smtp.AntiSpamSettings.ClientBlacklist.Length == 0 || ctx.Transaction.Client is null)
            return;

        var result = false;
        var clientUp = ctx.Transaction.Client.ToUpper();
        foreach (var client in C.Smtp.AntiSpamSettings.ClientBlacklist)
            if (result)
                break;
            else if (client.StartsWith('*'))
            {
                var asnEnd = client.TrimStart('*').ToUpper();
                result = clientUp.EndsWith(asnEnd);
            }
            else if (client.EndsWith('*'))
            {
                var asnStart = client.TrimEnd('*').ToUpper();
                result = clientUp.StartsWith(asnStart);
            }
            else
                result = clientUp == client.ToUpper();

        if (!result)
            return;

        ctx.Log("Matches Client blacklist");
        if (C.Smtp.AntiSpamSettings.BanClientBlacklist)
            BanIp(ctx, "Client blacklist");
        if (C.Smtp.AntiSpamSettings.TarpitClientBlacklist)
            await TarpitAsync(ctx);

        ctx.Log("Closing connection");
        throw new ResponseException(new Response(ReplyCode.TransactionFailed), true);
    }
    public static async Task CheckHeloEhloAsync(this SessionContext ctx)
    {
        if (ctx.Transaction.Secure || C.IsDebug)
            return;
        if (!C.Smtp.AntiSpamSettings.EnforceForwardDns && !C.Smtp.AntiSpamSettings.EnforceReverseDns)
            return;
        if (ctx.Transaction.IpAddress == null || !IPAddress.TryParse(ctx.Transaction.IpAddress, out var ip))
        {
            ctx.Log("No IP specified, closing connection");
            throw new ResponseException(new Response(ReplyCode.TransactionFailed, "Invalid IP"), true);
        }
        if (string.IsNullOrWhiteSpace(ctx.Transaction.Client) || !DomainName.TryParse(ctx.Transaction.Client, out var domain))
        {
            ctx.Log("No HELO/EHLO specified, closing connection");
            throw new ResponseException(new Response(ReplyCode.TransactionFailed, "Invalid HELO/EHLO"), true);
        }

        // Microsoft workaround since they don't care their servers can't complete FCrDNS
        if (C.Smtp.AntiSpamSettings.EnableFCrDNSWorkarounds && ctx.Transaction.Client.EndsWith(".outbound.protection.outlook.com"))
        {
            // Their SPF record should list all IP addresses their servers use
            var outlookDomain = DomainName.Parse("spf.protection.outlook.com");
            var dnsMessage = DnsClient.Default.Resolve(outlookDomain, RecordType.Txt);
            if (dnsMessage != null && dnsMessage.AnswerRecords.Count > 0 && (dnsMessage.ReturnCode == ReturnCode.NoError || dnsMessage.ReturnCode == ReturnCode.NxDomain))
                foreach (DnsRecordBase dnsRecord in dnsMessage.AnswerRecords)
                    if (dnsRecord is TxtRecord txtRecord && ARSoft.Tools.Net.Spf.SpfRecord.TryParse(txtRecord.TextData, out var spfRecord))
                        foreach (var term in spfRecord!.Terms)
                            if (term is SpfMechanism spf && spf.Qualifier == SpfQualifier.Pass && spf.Type == SpfMechanismType.Ip4)
                                if (IpService.IsInRange(ctx.Transaction.IpAddress, spf.Domain, spf.Prefix))
                                {
                                    ctx.Log("Outlook workaround succedded, skipping lookups");
                                    return;
                                }
        }

        // Forward DNS check
        bool? forwardSuccess = null;
        var fwdMessage = await DnsClient.Default.ResolveAsync(domain!);
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

        if (!forwardSuccess.HasValue)
            ctx.Log("Forward DNS lookup failed");
        else if (forwardSuccess.Value)
            ctx.Log("Forward DNS lookup matches IP");
        else if (!C.Smtp.AntiSpamSettings.EnforceForwardDns)
            ctx.Log("No forward DNS matches");
        else
        {
            ctx.Log("No forward DNS matches");
            if (C.Smtp.AntiSpamSettings.BanForwardDns)
                BanIp(ctx, "No forward DNS matches");
            if (C.Smtp.AntiSpamSettings.TarpitForwardDns)
                await TarpitAsync(ctx);

            ctx.Log("Closing connection");
            throw new ResponseException(new Response(ReplyCode.TransactionFailed, "Forward DNS lookup didn't match the IP"), true);
        }

        // Reverse DNS check
        bool? reverseSuccess = null;
        var rvsMessage = await DnsClient.Default.ResolveAsync(ip.GetReverseLookupDomain(), RecordType.Ptr);
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

        if (!reverseSuccess.HasValue)
            ctx.Log("Reverse DNS lookup failed");
        else if (reverseSuccess.Value)
            ctx.Log("Reverse DNS lookup matches HELO/EHLO");
        else if (!C.Smtp.AntiSpamSettings.EnforceReverseDns)
            ctx.Log("No reverse DNS matches");
        else
        {
            ctx.Log("No reverse DNS matches");
            if (C.Smtp.AntiSpamSettings.BanReverseDns)
                BanIp(ctx, "No reverse DNS matches");
            if (C.Smtp.AntiSpamSettings.TarpitReverseDns)
                await TarpitAsync(ctx);

            ctx.Log("Closing connection");
            throw new ResponseException(new Response(ReplyCode.TransactionFailed, "Reverse DNS lookup didn't match the HELO/EHLO"), true);
        }
    }
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

        List<string> listed = new(checks.Length), notListed = new(checks.Length), failed = new(checks.Length);
        foreach (var check in checks)
            if (!check.Listed.HasValue)
                failed.Add(check.Zone);
            else if (check.Listed.Value)
                listed.Add(check.Zone);
            else
                notListed.Add(check.Zone);

        if (listed.Count > notListed.Count && C.Smtp.AntiSpamSettings.EnforceDnsBlockList)
        {
            // TODO: figure out priority and then ban & tarpit
            var list = $"Listed at {string.Join(", ", listed)}";
            ctx.Log($"{list}, closing connection", new { listed, notListed, failed });
            throw new ResponseException(new Response(ReplyCode.TransactionFailed, list), true);
        }
        else
            ctx.Log($"RBL listed: {listed.Count}, not listed: {notListed.Count}, failed: {failed.Count}", new { listed, notListed, failed });
    }
    public static async Task CheckSpfAsync(this SessionContext ctx, string senderDomain)
    {
        if (C.IsDebug || string.IsNullOrWhiteSpace(ctx.Transaction.IpAddress))
            return;

        if (!IPAddress.TryParse(ctx.Transaction.IpAddress, out var ip))
        {
            ctx.Log("Invalid IP Address, can't check SPF");
            if (C.Smtp.AntiSpamSettings.EnforceSpf)
            {
                if (C.Smtp.AntiSpamSettings.BanSpf)
                    BanIp(ctx, "Invalid IP Address, can't check SPF");
                if (C.Smtp.AntiSpamSettings.TarpitSpf)
                    await TarpitAsync(ctx);

                ctx.Log("Closing connection");
                throw new ResponseException(new Response(ReplyCode.TransactionFailed), true);
            }
            return;
        }
        if (string.IsNullOrWhiteSpace(senderDomain) || !DomainName.TryParse(senderDomain, out var domain))
        {
            ctx.Log("Invalid MAIL FROM");
            if (C.Smtp.AntiSpamSettings.EnforceSpf)
            {
                if (C.Smtp.AntiSpamSettings.BanSpf)
                    BanIp(ctx, "Invalid MAIL FROM");
                if (C.Smtp.AntiSpamSettings.TarpitSpf)
                    await TarpitAsync(ctx);

                ctx.Log("Closing connection");
                throw new ResponseException(new Response(ReplyCode.TransactionFailed, "Invalid sender"), true);
            }
            return;
        }

        var validator = new SpfValidator();
        var result = await validator.CheckHostAsync(ip, domain!, string.Empty);
        ctx.Spf = result.Result;

        ctx.Log($"SPF {ctx.Spf}");
        switch (ctx.Spf)
        {
            case SpfQualifier.Pass:
                break;
            case SpfQualifier.Fail:
            case SpfQualifier.SoftFail:  //SPF softfail is interpreted in DMARC as fail by default
            case SpfQualifier.None:  //SPF none is treated as fail in DMARC
            case SpfQualifier.Neutral:  //SPF neutral is interpreted in DMARC as fail by default
            case SpfQualifier.TempError:  //the error is used to return a 4xx status code and the SMTP session ends
            case SpfQualifier.PermError:  //SPF permerror is interpreted in DMARC as fail
                if (C.Smtp.AntiSpamSettings.EnforceSpf)
                {
                    if (C.Smtp.AntiSpamSettings.BanSpf)
                        BanIp(ctx, $"SPF {ctx.Spf}");
                    if (C.Smtp.AntiSpamSettings.TarpitSpf)
                        await TarpitAsync(ctx);

                    ctx.Log("Closing connection");
                    throw new ResponseException(new Response(ReplyCode.TransactionFailed, $"SPF {ctx.Spf}"), true);
                }
                break;
        }
    }
    public static void LocalRecipientResolved(this SessionContext ctx) => ctx.ConsecutiveRcptFail = 0;
    public static async Task LocalRecipientNotResolvedAsnyc(this SessionContext ctx)
    {
        if (C.Smtp.AntiSpamSettings.ConsecutiveRcptFail == 0)
            return;

        if (C.Smtp.AntiSpamSettings.TarpitConsecutiveRcptFail)
        {
            var perTry = TimeSpan.FromSeconds(C.Smtp.AntiSpamSettings.TarpitSeconds)
                / C.Smtp.AntiSpamSettings.ConsecutiveRcptFail;
            var delay = perTry * ctx.ConsecutiveRcptFail;
            if (delay > TimeSpan.Zero)
            {
                ctx.Log($"Tarpit for {delay.TotalSeconds} seconds");
                await Task.Delay(delay);
            }
        }

        ctx.ConsecutiveRcptFail++;
        if (ctx.ConsecutiveRcptFail >= C.Smtp.AntiSpamSettings.ConsecutiveRcptFail)
        {
            if (C.Smtp.AntiSpamSettings.BanConsecutiveRcptFail)
                BanIp(ctx, "Consecutive RCPT command failed count over treshold");
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