namespace poshtar.Entities;

public class Transaction
{
    public int TransactionId { get; set; }
    public Guid ConnectionId { get; set; }
    public bool Submission { get; set; }
    public string? IpAddress { get; set; }
    public string? CountryCode { get; set; }
    public string? CountryName { get; set; }
    public string? Asn { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Client { get; set; }
    public int? FromUserId { get; set; }
    public string? From { get; set; }
    public bool Secure { get; set; }

    public User? FromUser { get; set; }
    public virtual List<LogEntry> Logs { get; set; } = new();
    public virtual List<Recipient> Recipients { get; set; } = new();

    public Dictionary<int, string> InternalUsers = new();
    public HashSet<string> ExternalAddresses = new(StringComparer.InvariantCultureIgnoreCase);
}