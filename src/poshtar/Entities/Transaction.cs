namespace poshtar.Entities;

public class Transaction
{
    public int TransactionId { get; set; }
    public Guid ConnectionId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Client { get; set; }
    public int? FromUserId { get; set; }
    public string? From { get; set; }
    public bool Complete { get; set; }

    public User? FromUser { get; set; }
    public virtual List<LogEntry> Logs { get; set; } = new();
    public virtual List<Recipient> Recipients { get; set; } = new();

    public HashSet<int> AddedRecipientIds = new();
}