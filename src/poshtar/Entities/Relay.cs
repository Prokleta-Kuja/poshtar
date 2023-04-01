using System.Diagnostics;

namespace poshtar.Entities;

[DebuggerDisplay("{Name}")]
public class Relay
{
    public int RelayId { get; set; }
    public required string Name { get; set; }
    public required string Host { get; set; }
    public int Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public DateTime? Disabled { get; set; }

    public virtual List<Domain> Domains { get; set; } = new();
}