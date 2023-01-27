using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Endpoints;

public class GetDomainById : IEndpointRequest<DomainByIdResponse>
{
    public int Id { get; set; }

    public async Task<DomainByIdResponse> HandleAsync(IServiceProvider sp)
    {
        var logger = sp.GetRequiredService<ILogger<GetDomainById>>();
        var db = sp.GetRequiredService<AppDbContext>();

        var domain = await db.Domains
            .Where(x => x.DomainId == Id)
            .Select(x => new DomainByIdResponse
            {
                DomainId = x.DomainId,
                Name = x.Name,
                Disabled = x.Disabled
            })
            .FirstOrDefaultAsync();

        if (domain == null)
            throw new NotFoundException();

        logger.LogInformation("OK");
        return domain;
    }

    public Dictionary<string, string[]> Validate(IServiceProvider sp)
    {
        return new(0);
    }
}
public record DomainByIdResponse
{
    [Required] public required int DomainId { get; init; } = default!;
    [Required] public required string Name { get; init; } = default!;
    public required DateTime? Disabled { get; init; }
    [Required] public Boja BojaMoja { get; set; } = Boja.Plava;
}

public enum Boja
{
    Crvena = 1,
    Plava = 2,
    Zelena = 3,
}