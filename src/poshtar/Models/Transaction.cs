using System.ComponentModel.DataAnnotations;

namespace poshtar.Models;

public class TransactionLM
{
    [Required] public required int Id { get; set; }
    [Required] public required Guid ConnectionId { get; set; }
    [Required] public required bool Submission { get; set; }
    public string? IpAddress { get; set; }
    public string? Country { get; set; }
    [Required] public required DateTime Start { get; set; }
    [Required] public required DateTime End { get; set; }
    public string? Client { get; set; }
    public string? Username { get; set; }
    public string? From { get; set; }
    [Required] public bool Queued { get; set; }
    [Required] public bool Secure { get; set; }
}

public class LogEntryLM
{
    [Required] public int Id { get; set; }
    [Required] public DateTime Timestamp { get; set; }
    [Required] public required string Message { get; set; }
    public string? Propertires { get; set; }
}

public class RecipientLM
{
    [Required] public int Id { get; set; }
    public int? UserId { get; set; }
    [Required] public required string Data { get; set; }
}