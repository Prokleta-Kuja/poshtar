using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace poshtar.Smtp;

public class EndpointDefinition
{
    public EndpointDefinition(int port, X509Certificate2 cert)
    {
        Endpoint = new IPEndPoint(IPAddress.Any, port);
        ServerCertificate = cert;
        ReadTimeout = TimeSpan.FromMinutes(2);
    }
    /// <summary>
    /// The IP endpoint to listen on.
    /// </summary>
    public IPEndPoint Endpoint { get; set; }

    /// <summary>
    /// The timeout on each individual buffer read.
    /// </summary>
    public TimeSpan ReadTimeout { get; set; }

    /// <summary>
    /// Gets the Server Certificate to use when starting a TLS session.
    /// </summary>
    public X509Certificate ServerCertificate { get; set; }
}