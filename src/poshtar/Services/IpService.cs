using System.Net;
using MaxMind.GeoIP2;

namespace poshtar.Services;

public class IpService
{
    public (string? code, string? name, string? asn) GetInfo(string ipAddress)
    {
        if (!string.IsNullOrWhiteSpace(C.MonitoringIp) && IsInRange(ipAddress, C.MonitoringIp))
            return (code: C.COUNTRY_CODE_MONITOR, name: C.MonitoringIp, asn: "Monitor");

        if (C.PrivateIpRanges.Any(range => IsInRange(ipAddress, range)))
            return (code: C.COUNTRY_CODE_PRIVATE, name: "Private range", asn: "LAN");

        (string? code, string? name, string? asn) result = new();
        if (File.Exists(C.Paths.MaxMindCountryDb))
            try
            {
                using var reader = new DatabaseReader(C.Paths.MaxMindCountryDb);
                var response = reader.Country(ipAddress);
                result.code = response?.Country?.IsoCode;
                result.name = response?.Country?.Name;
            }
            catch (Exception) { }

        var asn = string.Empty;
        if (File.Exists(C.Paths.MaxMindAsnDb))
            try
            {
                using var reader = new MaxMind.GeoIP2.DatabaseReader(C.Paths.MaxMindAsnDb);
                var response = reader.Asn(ipAddress);
                result.asn = response.AutonomousSystemOrganization;
            }
            catch (Exception) { }

        return result;
    }

    private bool IsInRange(string ipAddress, string CIDRmask)
    {
        try
        {
            string[] parts = CIDRmask.Split('/');

            int IP_addr = BitConverter.ToInt32(IPAddress.Parse(ipAddress).GetAddressBytes(), 0);
            int CIDR_addr = BitConverter.ToInt32(IPAddress.Parse(parts[0]).GetAddressBytes(), 0);
            int CIDR_mask = IPAddress.HostToNetworkOrder(-1 << (32 - int.Parse(parts[1])));

            return ((IP_addr & CIDR_mask) == (CIDR_addr & CIDR_mask));
        }
        catch (Exception)
        {
            return false;
        }
    }
}