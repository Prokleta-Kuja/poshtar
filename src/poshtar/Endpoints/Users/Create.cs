using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Services;

namespace poshtar.Endpoints;

public class CreateUser : IEndpointRequest<UserCreateResponse>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsMaster { get; set; }
    public int? Quota { get; set; }
    public required string Password { get; set; }

    public async Task<UserCreateResponse> HandleAsync(IServiceProvider sp)
    {
        using var db = sp.GetRequiredService<AppDbContext>();

        Name = Name.Trim().ToLower();
        var isDuplicate = await db.Users
            .AsNoTracking()
            .Where(d => d.Name == Name)
            .AnyAsync();

        if (isDuplicate)
            throw new ParamException(nameof(Name), "Already exists");

        var result = DovecotHasher.Hash(Password);
        var user = new User
        {
            Name = Name,
            Description = Description,
            IsMaster = IsMaster,
            Quota = Quota * 1024 * 1024,
            Salt = result.Salt,
            Hash = result.Hash,
            Password = DovecotHasher.Password(result.Salt, result.Hash),
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var response = new UserCreateResponse { UserId = user.UserId };
        return response;
    }

    public Dictionary<string, string> Validate(IServiceProvider sp)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add(nameof(Name), "Required");

        if (Quota.HasValue && Quota.Value < 10)
            errors.Add(nameof(Quota), "Must be greater than 10");

        if (string.IsNullOrWhiteSpace(Password))
            errors.Add(nameof(Password), "Required");

        return errors;
    }
}
public record UserCreateResponse
{
    [Required] public int UserId { get; set; }
}