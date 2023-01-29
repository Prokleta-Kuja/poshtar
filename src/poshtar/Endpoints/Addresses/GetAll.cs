using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Endpoints;

public class GetAddresses : ListRequest, IEndpointRequest<Response<Addresses>>
{
    public int? DomainId { get; set; }
    public int? UserId { get; set; }
    public async Task<Response<Addresses>> HandleAsync(IServiceProvider sp)
    {
        using var db = sp.GetRequiredService<AppDbContext>();
        var query = db.Addresses.Include(a => a.Domain).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
            query = query.Where(a => EF.Functions.Like(a.Pattern, $"%{SearchTerm}%") || EF.Functions.Like(a.Description!, $"%{SearchTerm}%"));

        if (DomainId.HasValue)
            query = query.Where(a => a.DomainId == DomainId.Value);

        if (UserId.HasValue)
            query = query.Where(a => a.Users.Any(u => u.UserId == UserId.Value));

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(SortBy) && Enum.TryParse<AddressesSortBy>(SortBy, true, out var sortBy))
            query = sortBy switch
            {
                AddressesSortBy.Pattern => query.Order(a => a.Pattern, Ascending),
                AddressesSortBy.Description => query.Order(a => a.Description, Ascending),
                AddressesSortBy.IsStatic => query.Order(a => a.IsStatic, Ascending),
                AddressesSortBy.Domain => query.Order(a => a.Domain!.Name, Ascending),
                _ => query
            };

        var items = await query
            .Paginate(this)
            .Select(a => new Addresses
            {
                AddressId = a.AddressId,
                DomainId = a.DomainId,
                Pattern = a.Pattern,
                Description = a.Description,
                IsStatic = a.IsStatic,
                Domain = a.Domain!.Name,
                UserCount = a.Users.Count,
                Disabled = a.Disabled,
            })
            .ToListAsync();

        return new(this, count, items);
    }
}
public record Addresses
{
    [Required] public int AddressId { get; set; }
    [Required] public int DomainId { get; set; }
    [Required] public required string Pattern { get; set; }
    public string? Description { get; set; }
    [Required] public bool IsStatic { get; set; }
    [Required] public required string Domain { get; set; }
    [Required] public int UserCount { get; set; }
    public required DateTime? Disabled { get; init; }
}

public enum AddressesSortBy
{
    Pattern = 0,
    Description = 1,
    IsStatic = 2,
    Domain = 3,
}