using System.Net;
using MaxMind.GeoIP2;

namespace poshtar.Services;

public class IpService
{
    public string? GetCountry(string ipAddress)
    {
        if (!string.IsNullOrWhiteSpace(C.MonitoringIp) && IsInRange(ipAddress, C.MonitoringIp))
            return "ZZ";

        if (C.PrivateIpRanges.Any(range => IsInRange(ipAddress, range)))
            return "XX";

        if (!File.Exists(C.Paths.MaxMindDb))
            return null;

        try
        {
            using var reader = new DatabaseReader(C.Paths.MaxMindDb);
            var response = reader.Country(ipAddress);
            return response?.Country?.IsoCode;
        }
        catch (Exception)
        {
            return null;
        }
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