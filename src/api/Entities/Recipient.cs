using System.Text.Json;

namespace poshtar.Entities;

public class Recipient
{
    public Recipient() { Data = null!; }
    public Recipient(int userId, string name)
    {
        UserId = userId;
        Data = name;
    }
    public Recipient(HashSet<string> externalRecipients)
    {
        Data = JsonSerializer.Serialize(externalRecipients);
    }
    public int RecipientId { get; set; }
    public int TransactionId { get; set; }
    public int? UserId { get; set; }
    public string Data { get; set; }
    public bool Delivered { get; set; }

    public User? User { get; set; }
    public Transaction? Transaction { get; set; }
}