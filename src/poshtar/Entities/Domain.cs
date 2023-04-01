using System.Diagnostics;

namespace poshtar.Entities;

[DebuggerDisplay("{Name}")]
public class Domain
{
    public int DomainId { get; set; }
    public required string Name { get; set; }
    public int? RelayId { get; set; }
    public DateTime? Disabled { get; set; }

    public Relay? Relay { get; set; }
    public virtual List<Address> Addresses { get; set; } = new();
}