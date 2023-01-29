using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Endpoints;

public class GetUsers : ListRequest, IEndpointRequest<Response<Users>>
{
    public int? DomainId { get; set; }
    public int? AddressId { get; set; }
    public async Task<Response<Users>> HandleAsync(IServiceProvider sp)
    {
        using var db = sp.GetRequiredService<AppDbContext>();
        var query = db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
            query = query.Where(u => u.Name.Contains(SearchTerm.ToLower()) || u.Description!.Contains(SearchTerm));

        if (DomainId.HasValue)
            query = query.Where(u => u.Domains.Any(d => d.DomainId == DomainId.Value));

        if (AddressId.HasValue)
            query = query.Where(u => u.Addresses.Any(a => a.AddressId == AddressId.Value));

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(SortBy) && Enum.TryParse<UsersSortBy>(SortBy, true, out var sortBy))
            query = sortBy switch
            {
                UsersSortBy.Name => query.Order(u => u.Name, Ascending),
                UsersSortBy.Description => query.Order(u => u.Description, Ascending),
                UsersSortBy.IsMaster => query.Order(u => u.IsMaster, Ascending),
                UsersSortBy.Quota => query.Order(u => u.Quota, Ascending),
                _ => query
            };

        var items = await query
            .Paginate(this)
            .Select(u => new Users
            {
                UserId = u.UserId,
                Name = u.Name,
                Description = u.Description,
                IsMaster = u.IsMaster,
                Quota = u.Quota.HasValue ? u.Quota / 1024 / 1024 : null,
                AddressCount = u.Addresses.Count,
                DomainCount = u.Domains.Count,
                Disabled = u.Disabled,
            })
            .ToListAsync();

        return new(this, count, items);
    }
}
public record Users
{
    [Required] public int UserId { get; set; }
    [Required] public required string Name { get; set; }
    public string? Description { get; set; }
    [Required] public bool IsMaster { get; set; }
    public int? Quota { get; set; }
    public int AddressCount { get; set; }
    public int DomainCount { get; set; }
    public required DateTime? Disabled { get; init; }
}

public enum UsersSortBy
{
    Name = 0,
    Description = 1,
    IsMaster = 2,
    Quota = 3,
}