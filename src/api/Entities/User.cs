using System.Diagnostics;

namespace poshtar.Entities;

[DebuggerDisplay("Name={Name} Master={IsMaster}")]
public class User
{
    public int UserId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsMaster { get; set; }
    public int? Quota { get; set; }
    public required string Salt { get; set; }
    public required string Hash { get; set; }
    public required string Password { get; set; }
    public byte[]? OtpKey { get; set; }
    public DateTime? Disabled { get; set; }

    public virtual List<Address> Addresses { get; set; } = new();
    public virtual List<Transaction> Transactions { get; set; } = new();
    public virtual List<CalendarUser> CalendarUsers { get; set; } = new();
}