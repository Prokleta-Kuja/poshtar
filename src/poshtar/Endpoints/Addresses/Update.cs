using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Endpoints;

public class UpdateAddress : IEndpointRequest<AddressUpdateResponse>
{
    public int AddressId { get; set; }
    public int DomainId { get; set; }
    public required string Pattern { get; set; }
    public string? Description { get; set; }
    public bool IsStatic { get; set; }
    public bool ToggleDisabled { get; set; }

    public async Task<AddressUpdateResponse> HandleAsync(IServiceProvider sp)
    {
        using var db = sp.GetRequiredService<AppDbContext>();

        if (IsStatic)
            Pattern = Pattern.Trim().ToLower();

        var isDuplicate = await db.Addresses
            .AsNoTracking()
            .Where(a => a.Pattern == Pattern && a.DomainId == DomainId && a.AddressId != AddressId)
            .AnyAsync();

        if (isDuplicate)
            throw new ParamException(nameof(Pattern), "Already exists");

        var address = await db.Addresses.SingleOrDefaultAsync(a => a.AddressId == AddressId);

        if (address == null)
            throw new NotFoundException();

        address.Pattern = Pattern;
        address.Description = Description;
        address.IsStatic = IsStatic;
        if (ToggleDisabled)
            address.Disabled = address.Disabled.HasValue ? null : DateTime.UtcNow;

        await db.SaveChangesAsync();

        var response = new AddressUpdateResponse(address);
        return response;
    }

    public Dictionary<string, string> Validate(IServiceProvider sp)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(Pattern))
            errors.Add(nameof(Pattern), "Required");

        return errors;
    }
}
public record AddressUpdateResponse
{
    public AddressUpdateResponse(Address a)
    {
        AddressId = a.AddressId;
        DomainId = a.DomainId;
        Pattern = a.Pattern;
        Description = a.Description;
        IsStatic = a.IsStatic;
        Disabled = a.Disabled;
    }
    [Required] public int AddressId { get; set; }
    [Required] public int DomainId { get; set; }
    [Required] public string Pattern { get; set; }
    public string? Description { get; set; }
    [Required] public bool IsStatic { get; set; }
    public DateTime? Disabled { get; set; }
}