using poshtar.Entities;

namespace poshtar.Models;

public class UserVM
{
    public UserVM(User u)
    {
        Id = u.UserId;
        Name = u.Name;
        Description = u.Description;
        IsMaster = u.IsMaster;
        QuotaMegaBytes = u.Quota / 1024 / 1024;
        Disabled = u.Disabled;
    }
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsMaster { get; set; }
    public int? QuotaMegaBytes { get; set; }
    public DateTime? Disabled { get; set; }
}

public class UserLM
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsMaster { get; set; }
    public int? QuotaMegaBytes { get; set; }
    public DateTime? Disabled { get; set; }
    public int AddressCount { get; set; }
    public int DomainCount { get; set; }
}

public class UserCM
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsMaster { get; set; }
    public int? Quota { get; set; }
    public required string Password { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();

        if (string.IsNullOrWhiteSpace(Name))
            errorModel.Errors.Add(nameof(Name), "Required");

        if (Quota.HasValue && Quota.Value < 1)
            errorModel.Errors.Add(nameof(Quota), "Must be greater than 1");

        if (string.IsNullOrWhiteSpace(Password))
            errorModel.Errors.Add(nameof(Password), "Required");

        return errorModel.Errors.Count > 0;
    }
}

public class UserUM
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsMaster { get; set; }
    public int? Quota { get; set; }
    public string? NewPassword { get; set; }
    public bool? Disabled { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();

        if (string.IsNullOrWhiteSpace(Name))
            errorModel.Errors.Add(nameof(Name), "Required");

        if (Quota.HasValue && Quota.Value < 1)
            errorModel.Errors.Add(nameof(Quota), "Must be greater than 1");

        return errorModel.Errors.Count > 0;
    }
}