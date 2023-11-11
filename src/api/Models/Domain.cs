using System.ComponentModel.DataAnnotations;
using poshtar.Entities;

namespace poshtar.Models;

public class DomainVM
{
    public DomainVM(Domain d)
    {
        Id = d.DomainId;
        RelayId = d.RelayId;
        Name = d.Name;
        Disabled = d.Disabled;
    }
    [Required] public int Id { get; set; }
    public int? RelayId { get; set; }
    [Required] public string Name { get; set; }
    public DateTime? Disabled { get; set; }
}

public class DomainLM
{
    [Required] public required int Id { get; set; }
    [Required] public required string Name { get; set; }
    public int? RelayId { get; set; }
    public string? RelayName { get; set; }
    public required DateTime? Disabled { get; set; }
    [Required] public required int AddressCount { get; set; }
}

public class DomainCM
{
    [Required] public required string Name { get; set; }
    public int? RelayId { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();

        if (string.IsNullOrWhiteSpace(Name))
            errorModel.Errors.Add(nameof(Name), "Required");

        return errorModel.Errors.Count > 0;
    }
}

public class DomainUM
{
    [Required] public required string Name { get; set; }
    public int? RelayId { get; set; }
    public bool? Disabled { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();

        if (string.IsNullOrWhiteSpace(Name))
            errorModel.Errors.Add(nameof(Name), "Required");

        return errorModel.Errors.Count > 0;
    }
}