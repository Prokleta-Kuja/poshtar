using System.ComponentModel.DataAnnotations;
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
    [Required] public int LogEntryId { get; set; }
    [Required] public Guid Context { get; set; }
    [Required] public DateTime Timestamp { get; set; }
    [Required] public string Message { get; set; } = string.Empty;
    public string? Properties { get; set; }
}