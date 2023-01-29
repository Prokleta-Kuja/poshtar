using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Endpoints;

public class CreateAddress : IEndpointRequest<AddressCreateResponse>
{
    public int DomainId { get; set; }
    public required string Pattern { get; set; }
    public string? Description { get; set; }
    public bool IsStatic { get; set; }

    public async Task<AddressCreateResponse> HandleAsync(IServiceProvider sp)
    {
        using var db = sp.GetRequiredService<AppDbContext>();

        if (IsStatic)
            Pattern = Pattern.Trim().ToLower();

        var isDuplicate = await db.Addresses
            .AsNoTracking()
            .Where(a => a.Pattern == Pattern && a.DomainId == DomainId)
            .AnyAsync();

        if (isDuplicate)
            throw new ParamException(nameof(Pattern), "Already exists");

        var domain = await db.Domains.FirstOrDefaultAsync(d => d.DomainId == DomainId);
        if (domain == null)
            throw new ParamException(nameof(Pattern), "Invalid");

        var address = new Address
        {
            Pattern = Pattern,
            Description = Description,
            IsStatic = IsStatic,
        };

        domain.Addresses.Add(address);
        await db.SaveChangesAsync();

        var response = new AddressCreateResponse { AddressId = address.AddressId };
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
public record AddressCreateResponse
{
    [Required] public int AddressId { get; set; }
}