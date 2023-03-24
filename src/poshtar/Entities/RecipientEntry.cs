using System.Text.Json;

namespace poshtar.Entities;

public class RecipientEntry
{
    public RecipientEntry() { }
    public RecipientEntry(Guid contextId, int userId, string name)
    {
        ContextId = contextId;
        UserId = userId;
        Data = name;
    }
    public RecipientEntry(Guid contextId, IEnumerable<string> externalRecipients)
    {
        ContextId = contextId;
        Data = JsonSerializer.Serialize(externalRecipients);
    }
    public Guid ContextId { get; set; }
    public int? UserId { get; set; }
    public string Data { get; set; } = string.Empty;

    public User? User { get; set; }
}