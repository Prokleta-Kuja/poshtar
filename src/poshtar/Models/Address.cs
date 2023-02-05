using poshtar.Entities;

namespace poshtar.Models;

public class AddressVM
{
    public AddressVM(Address a)
    {
        Id = a.AddressId;
        DomainId = a.DomainId;
        Pattern = a.Pattern;
        Description = a.Description;
        IsStatic = a.IsStatic;
        Disabled = a.Disabled;
    }
    public int Id { get; set; }
    public int DomainId { get; set; }
    public string Pattern { get; set; }
    public string? Description { get; set; }
    public bool IsStatic { get; set; }
    public DateTime? Disabled { get; set; }
}

public class AddressLM
{
    public int Id { get; set; }
    public int DomainId { get; set; }
    public required string DomainName { get; set; }
    public required string Pattern { get; set; }
    public string? Description { get; set; }
    public bool IsStatic { get; set; }
    public DateTime? Disabled { get; set; }
    public required int UserCount { get; set; }
}

public class AddressCM
{
    public int DomainId { get; set; }
    public required string Pattern { get; set; }
    public string? Description { get; set; }
    public bool IsStatic { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();

        if (string.IsNullOrWhiteSpace(Pattern))
            errorModel.Errors.Add(nameof(Pattern), "Required");

        return errorModel.Errors.Count > 0;
    }
}

public class AddressUM
{
    public int DomainId { get; set; }
    public required string Pattern { get; set; }
    public string? Description { get; set; }
    public bool IsStatic { get; set; }
    public bool? Disabled { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();

        if (string.IsNullOrWhiteSpace(Pattern))
            errorModel.Errors.Add(nameof(Pattern), "Required");

        return errorModel.Errors.Count > 0;
    }
}