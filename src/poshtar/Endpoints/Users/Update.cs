using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Services;

namespace poshtar.Endpoints;

public class UpdateUser : IEndpointRequest<UserUpdateResponse>
{
    public int UserId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsMaster { get; set; }
    public int? Quota { get; set; }
    public string? NewPassword { get; set; }
    public bool ToggleDisabled { get; set; }

    public async Task<UserUpdateResponse> HandleAsync(IServiceProvider sp)
    {
        using var db = sp.GetRequiredService<AppDbContext>();

        Name = Name.Trim().ToLower();
        var user = await db.Users.FirstOrDefaultAsync(d => d.UserId == UserId);

        if (user == null)
            throw new NotFoundException();

        user.Name = Name;
        user.Description = Description;
        user.IsMaster = IsMaster;
        user.Quota = Quota * 1024 * 1024;
        if (!string.IsNullOrWhiteSpace(NewPassword))
        {
            var result = DovecotHasher.Hash(NewPassword);
            user.Salt = result.Salt;
            user.Hash = result.Hash;
            user.Password = DovecotHasher.Password(result.Salt, result.Hash);
        }
        if (ToggleDisabled)
            user.Disabled = user.Disabled.HasValue ? null : DateTime.UtcNow;

        await db.SaveChangesAsync();

        var response = new UserUpdateResponse(user);
        return response;
    }

    public Dictionary<string, string> Validate(IServiceProvider sp)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add(nameof(Name), "Required");

        if (Quota.HasValue && Quota.Value < 10)
            errors.Add(nameof(Quota), "Must be greater than 10 MB");

        return errors;
    }
}
public record UserUpdateResponse
{
    public UserUpdateResponse(User u)
    {
        UserId = u.UserId;
        Name = u.Name;
        Description = u.Description;
        IsMaster = u.IsMaster;
        Quota = u.Quota;
        Disabled = u.Disabled;
    }
    [Required] public int UserId { get; set; }
    [Required] public string Name { get; set; }
    public string? Description { get; set; }
    [Required] public bool IsMaster { get; set; }
    public int? Quota { get; set; }
    public DateTime? Disabled { get; set; }
}