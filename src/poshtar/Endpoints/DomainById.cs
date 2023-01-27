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
                Host = x.Host,
                Port = x.Port,
                IsSecure = x.IsSecure,
                Username = x.Username,
                Disabled = x.Disabled,
                AddressCount = x.Addresses.Count,
                UserCount = x.Users.Count,

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
    [Required] public required int DomainId { get; init; }
    [Required] public required string Name { get; init; }
    public required string Host { get; set; }
    public int Port { get; set; }
    public bool IsSecure { get; set; }
    public required string Username { get; set; }
    public DateTime? Disabled { get; set; }
    public int AddressCount { get; set; }
    public int UserCount { get; set; }
}