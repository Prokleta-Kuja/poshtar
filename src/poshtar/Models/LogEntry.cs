using Microsoft.Extensions.Logging.Abstractions;
using poshtar.Entities;

namespace poshtar.Models;

public class LogEntryVM
{
    public LogEntryVM(LogEntry le)
    {
        LogEntryId = le.LogEntryId;
        Context = le.Context;
        Timestamp = le.Timestamp;
        Message = le.Message;
        Properties = le.Properties;
    }
    public int LogEntryId { get; set; }
    public Guid Context { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Properties { get; set; }
}