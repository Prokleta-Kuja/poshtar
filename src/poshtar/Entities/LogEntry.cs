using System.Diagnostics;
using System.Text.Json;

namespace poshtar.Entities;

[DebuggerDisplay("{Message}")]
public class LogEntry
{
    public LogEntry() { }
    public LogEntry(string message, object? properties = null)
    {
        Timestamp = DateTime.UtcNow;
        Message = message;
        if (properties != null)
            Properties = JsonSerializer.Serialize(properties);
    }
    public int LogEntryId { get; set; }
    public int TransactionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Properties { get; set; }

    public Transaction? Transaction { get; set; }
}