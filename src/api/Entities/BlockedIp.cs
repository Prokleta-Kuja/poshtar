namespace poshtar.Entities;

public class BlockedIp
{
  public required string Address { get; set; }
  public required string Reason { get; set; }
  public DateTime BlockedOn { get; set; }
  public DateTime LastHit { get; set; }
}