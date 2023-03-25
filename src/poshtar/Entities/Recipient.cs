using System.Text.Json;

namespace poshtar.Entities;

public class Recipient
{
    public Recipient() { }
    public Recipient(int userId, string name)
    {
        UserId = userId;
        Data = name;
    }
    public Recipient(IEnumerable<string> externalRecipients)
    {
        Data = JsonSerializer.Serialize(externalRecipients);
    }
    public int RecipientEntryId { get; set; }
    public int TransactionId { get; set; }
    public int? UserId { get; set; }
    public string Data { get; set; } = string.Empty;

    public User? User { get; set; }
    public Transaction? Transaction { get; set; }
}