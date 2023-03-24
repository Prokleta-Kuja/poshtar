using System.Diagnostics;
using System.Text.Json;

namespace poshtar.Entities;

[DebuggerDisplay("{Message}")]
public class LogEntry
{
    public LogEntry() { }
    public LogEntry(Guid contextId, string message, object? properties)
    {
        ContextId = contextId;
        Timestamp = DateTime.UtcNow;
        Message = message;
        if (properties != null)
            Properties = JsonSerializer.Serialize(properties);
    }
    public Guid ContextId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Properties { get; set; }
}