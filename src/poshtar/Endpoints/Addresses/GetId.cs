using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Endpoints;

public class GetAddressById : IEndpointRequest<AddressByIdResponse>
{
    public int Id { get; set; }

    public async Task<AddressByIdResponse> HandleAsync(IServiceProvider sp)
    {
        using var db = sp.GetRequiredService<AppDbContext>();

        var address = await db.Addresses
            .Where(a => a.AddressId == Id)
            .Select(a => new AddressByIdResponse
            {
                AddressId = a.AddressId,
                DomainId = a.DomainId,
                Pattern = a.Pattern,
                Description = a.Description,
                IsStatic = a.IsStatic,
                Disabled = a.Disabled,
            })
            .FirstOrDefaultAsync();

        if (address == null)
            throw new NotFoundException();

        return address;
    }

    public Dictionary<string, string[]> Validate(IServiceProvider sp)
    {
        return new(0);
    }
}
public record AddressByIdResponse
{
    [Required] public int AddressId { get; set; }
    [Required] public int DomainId { get; set; }
    [Required] public required string Pattern { get; set; }
    public string? Description { get; set; }
    [Required] public bool IsStatic { get; set; }
    public DateTime? Disabled { get; set; }
}