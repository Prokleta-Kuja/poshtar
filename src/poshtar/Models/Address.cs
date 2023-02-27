using System.ComponentModel.DataAnnotations;
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
        Type = a.Type;
        Disabled = a.Disabled;
    }
    [Required] public int Id { get; set; }
    [Required] public int DomainId { get; set; }
    [Required] public string Pattern { get; set; }
    public string? Description { get; set; }
    [Required] public AddressType Type { get; set; }
    public DateTime? Disabled { get; set; }
}

public class AddressLM
{
    [Required] public int Id { get; set; }
    [Required] public int DomainId { get; set; }
    [Required] public required string DomainName { get; set; }
    [Required] public required string Pattern { get; set; }
    public string? Description { get; set; }
    [Required] public AddressType Type { get; set; }
    public DateTime? Disabled { get; set; }
    [Required] public required int UserCount { get; set; }
}

public class AddressCM
{
    [Required] public int DomainId { get; set; }
    [Required] public required string Pattern { get; set; }
    public string? Description { get; set; }
    [Required] public AddressType Type { get; set; }
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
    [Required] public int DomainId { get; set; }
    [Required] public required string Pattern { get; set; }
    public string? Description { get; set; }
    [Required] public AddressType Type { get; set; }
    public bool? Disabled { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();

        if (string.IsNullOrWhiteSpace(Pattern))
            errorModel.Errors.Add(nameof(Pattern), "Required");

        return errorModel.Errors.Count > 0;
    }
}