using System.Diagnostics;

namespace poshtar.Entities;

[DebuggerDisplay("Id={AddressId} Pattern={Pattern}")]
public class Address
{
    public int AddressId { get; set; }
    public int DomainId { get; set; }
    public required string Pattern { get; set; }
    public string? Description { get; set; }
    public AddressType Type { get; set; }
    public string? Expression { get; set; }
    public DateTime? Disabled { get; set; }

    public Domain? Domain { get; set; }
    public virtual List<User> Users { get; set; } = new();
}

public enum AddressType
{
    Exact = 0,
    Prefix = 1,
    Suffix = 2,
    CatchAll = 3,
}