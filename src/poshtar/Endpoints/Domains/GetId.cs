using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Endpoints;

public class GetDomainById : IEndpointRequest<DomainByIdResponse>
{
    public int DomainId { get; set; }

    public async Task<DomainByIdResponse> HandleAsync(IServiceProvider sp)
    {
        using var db = sp.GetRequiredService<AppDbContext>();

        var domain = await db.Domains
            .Where(d => d.DomainId == DomainId)
            .Select(d => new DomainByIdResponse
            {
                DomainId = d.DomainId,
                Name = d.Name,
                Host = d.Host,
                Port = d.Port,
                IsSecure = d.IsSecure,
                Username = d.Username,
                Disabled = d.Disabled,
                AddressCount = d.Addresses.Count,
                UserCount = d.Users.Count,

            })
            .FirstOrDefaultAsync();

        if (domain == null)
            throw new NotFoundException();

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