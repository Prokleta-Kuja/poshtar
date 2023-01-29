using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Endpoints;

public class GetUserById : IEndpointRequest<UserByIdResponse>
{
    public int UserId { get; set; }

    public async Task<UserByIdResponse> HandleAsync(IServiceProvider sp)
    {
        using var db = sp.GetRequiredService<AppDbContext>();

        var User = await db.Users
            .Where(u => u.UserId == UserId)
            .Select(u => new UserByIdResponse
            {
                UserId = u.UserId,
                Name = u.Name,
                Description = u.Description,
                IsMaster = u.IsMaster,
                Quota = u.Quota.HasValue ? u.Quota / 1024 / 1024 : null,
                Disabled = u.Disabled,
            })
            .FirstOrDefaultAsync();

        if (User == null)
            throw new NotFoundException();

        return User;
    }

    public Dictionary<string, string[]> Validate(IServiceProvider sp)
    {
        return new(0);
    }
}
public record UserByIdResponse
{
    [Required] public int UserId { get; set; }
    [Required] public required string Name { get; set; }
    public string? Description { get; set; }
    [Required] public bool IsMaster { get; set; }
    public int? Quota { get; set; }
    public DateTime? Disabled { get; set; }
}