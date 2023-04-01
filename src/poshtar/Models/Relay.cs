using System.ComponentModel.DataAnnotations;
using poshtar.Entities;

namespace poshtar.Models;

public class RelayVM
{
    public RelayVM(Relay r)
    {
        Id = r.RelayId;
        Name = r.Name;
        Host = r.Host;
        Port = r.Port;
        Username = r.Username;
        Disabled = r.Disabled;
    }
    [Required] public int Id { get; set; }
    [Required] public string Name { get; set; }
    [Required] public string Host { get; set; }
    [Required] public int Port { get; set; }
    [Required] public string Username { get; set; }
    public DateTime? Disabled { get; set; }
}

public class RelayLM
{
    [Required] public required int Id { get; set; }
    [Required] public required string Name { get; set; }
    [Required] public required string Username { get; set; }
    [Required] public required string Host { get; set; }
    [Required] public required int Port { get; set; }
    public required DateTime? Disabled { get; set; }
    [Required] public required int DomainCount { get; set; }
}

public class RelayCM
{
    [Required] public required string Name { get; set; }
    [Required] public required string Host { get; set; }
    [Required] public int Port { get; set; }
    [Required] public required string Username { get; set; }
    [Required] public required string Password { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();

        if (string.IsNullOrWhiteSpace(Name))
            errorModel.Errors.Add(nameof(Name), "Required");

        if (string.IsNullOrWhiteSpace(Host))
            errorModel.Errors.Add(nameof(Host), "Required");

        if (Port <= 0 || Port > 65535)
            errorModel.Errors.Add(nameof(Port), "Invalid");

        if (string.IsNullOrWhiteSpace(Username))
            errorModel.Errors.Add(nameof(Username), "Required");

        if (string.IsNullOrWhiteSpace(Password))
            errorModel.Errors.Add(nameof(Password), "Required");

        return errorModel.Errors.Count > 0;
    }
}

public class RelayUM
{
    [Required] public required string Name { get; set; }
    [Required] public required string Host { get; set; }
    [Required] public int Port { get; set; }
    [Required] public required string Username { get; set; }
    public string? NewPassword { get; set; }
    public bool? Disabled { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();

        if (string.IsNullOrWhiteSpace(Name))
            errorModel.Errors.Add(nameof(Name), "Required");

        if (string.IsNullOrWhiteSpace(Host))
            errorModel.Errors.Add(nameof(Host), "Required");

        if (Port <= 0 || Port > 65535)
            errorModel.Errors.Add(nameof(Port), "Invalid");

        if (string.IsNullOrWhiteSpace(Username))
            errorModel.Errors.Add(nameof(Username), "Required");

        return errorModel.Errors.Count > 0;
    }
}