using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Endpoints;

public class GetDomains : ListRequest, IEndpointRequest<Response<Domains>>
{
    public string? SearchTerm { get; set; }
    public async Task<Response<Domains>> HandleAsync(IServiceProvider sp)
    {
        using var db = sp.GetRequiredService<AppDbContext>();
        var query = db.Domains.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
            query = query.Where(d => EF.Functions.Like(d.Name, $"%{SearchTerm}%") || EF.Functions.Like(d.Host, $"%{SearchTerm}%"));

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(SortBy) && Enum.TryParse<DomainsSortBy>(SortBy, true, out var sortBy))
            query = sortBy switch
            {
                DomainsSortBy.Name => query.Order(d => d.Name, Ascending),
                DomainsSortBy.Host => query.Order(d => d.Host, Ascending),
                DomainsSortBy.AddressCount => query.Order(d => d.Addresses.Count(), Ascending),
                DomainsSortBy.UserCount => query.Order(d => d.Users.Count(), Ascending),
                _ => query
            };

        var items = await query
            .Paginate(this)
            .Select(d => new Domains
            {
                DomainId = d.DomainId,
                Name = d.Name,
                Host = d.Host,
                AddressCount = d.Addresses.Count(),
                UserCount = d.Users.Count(),
                Disabled = d.Disabled
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