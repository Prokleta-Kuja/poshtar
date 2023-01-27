using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Endpoints;

public class GetDomains : ListRequest, IEndpointRequest<Response<Domains>>
{
    public string? SearchTerm { get; set; }
    public async Task<Response<Domains>> HandleAsync(IServiceProvider sp)
    {
        var logger = sp.GetRequiredService<ILogger<GetDomains>>();
        var db = sp.GetRequiredService<AppDbContext>();
        var query = db.Domains.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
            query.Where(x => x.Name.Contains(SearchTerm) || x.Host.Contains(SearchTerm));

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(SortBy) && Enum.TryParse<DomainsSortBy>(SortBy, true, out var sortBy))
            query = sortBy switch
            {
                DomainsSortBy.Name => query.Order(x => x.Name, Ascending),
                DomainsSortBy.Host => query.Order(x => x.Host, Ascending),
                DomainsSortBy.AddressCount => query.Order(x => x.Addresses.Count(), Ascending),
                DomainsSortBy.UserCount => query.Order(x => x.Users.Count(), Ascending),
                _ => query
            };

        var items = await query
            .Paginate(this)
            .Select(x => new Domains
            {
                DomainId = x.DomainId,
                Name = x.Name,
                Host = x.Host,
                AddressCount = x.Addresses.Count(),
                UserCount = x.Users.Count(),
                Disabled = x.Disabled
            })
            .ToListAsync();

        return new(this, count, items);
    }
}
public record Domains
{
    [Required] public required int DomainId { get; init; }
    [Required] public required string Name { get; init; }
    [Required] public required string Host { get; init; }
    [Required] public required int AddressCount { get; init; }
    [Required] public required int UserCount { get; init; }
    public required DateTime? Disabled { get; init; }
}

public enum DomainsSortBy
{
    Name = 0,
    Host = 1,
    AddressCount = 2,
    UserCount = 3,
}