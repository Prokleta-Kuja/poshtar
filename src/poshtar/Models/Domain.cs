using poshtar.Entities;

namespace poshtar.Models;

public class DomainVM
{
    public DomainVM(Domain d)
    {
        Id = d.DomainId;
        Name = d.Name;
        Host = d.Host;
        Port = d.Port;
        IsSecure = d.IsSecure;
        Username = d.Username;
        Disabled = d.Disabled;
    }
    public int Id { get; set; }
    public string Name { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public bool IsSecure { get; set; }
    public string Username { get; set; }
    public DateTime? Disabled { get; set; }
}

public class DomainLM
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Host { get; set; }
    public required int UserCount { get; set; }
    public required int AddressCount { get; set; }
}

public class DomainCM
{
    public required string Name { get; set; }
    public required string Host { get; set; }
    public int Port { get; set; }
    public bool IsSecure { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
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

public class DomainUM
{
    public required string Name { get; set; }
    public required string Host { get; set; }
    public int Port { get; set; }
    public bool IsSecure { get; set; }
    public required string Username { get; set; }
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