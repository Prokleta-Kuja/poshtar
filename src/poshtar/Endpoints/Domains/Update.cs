using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Endpoints;

public class UpdateDomain : IEndpointRequest<DomainUpdateResponse>
{
    public int DomainId { get; set; }
    public required string Name { get; set; }
    public required string Host { get; set; }
    public int Port { get; set; }
    public bool IsSecure { get; set; }
    public required string Username { get; set; }
    public string? NewPassword { get; set; }
    public bool? Disabled { get; set; }

    public async Task<DomainUpdateResponse> HandleAsync(IServiceProvider sp)
    {
        using var db = sp.GetRequiredService<AppDbContext>();

        Name = Name.Trim().ToLower();
        var domain = await db.Domains.FirstOrDefaultAsync(d => d.DomainId == DomainId);

        if (domain == null)
            throw new NotFoundException();

        domain.Name = Name;
        domain.Host = Host;
        domain.Port = Port;
        domain.IsSecure = IsSecure;
        domain.Username = Username;
        if (!string.IsNullOrWhiteSpace(NewPassword))
        {
            var dpProvider = sp.GetRequiredService<IDataProtectionProvider>();
            var serverProtector = dpProvider.CreateProtector(nameof(Domain));
            domain.Password = serverProtector.Protect(NewPassword);
        }
        if (Disabled.HasValue)
            domain.Disabled = Disabled.Value ? domain.Disabled.HasValue ? domain.Disabled : DateTime.UtcNow : null;

        await db.SaveChangesAsync();

        var response = new DomainUpdateResponse(domain);
        return response;
    }

    public Dictionary<string, string> Validate(IServiceProvider sp)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add(nameof(Name), "Required");

        if (string.IsNullOrWhiteSpace(Host))
            errors.Add(nameof(Host), "Required");

        if (Port <= 0 || Port > 65535)
            errors.Add(nameof(Port), "Invalid");

        if (string.IsNullOrWhiteSpace(Username))
            errors.Add(nameof(Username), "Required");

        return errors;
    }
}
public record DomainUpdateResponse
{
    public DomainUpdateResponse(Domain d)
    {
        DomainId = d.DomainId;
        Name = d.Name;
        Host = d.Host;
        Port = d.Port;
        IsSecure = d.IsSecure;
        Username = d.Username;
        Disabled = d.Disabled;
    }
    [Required] public int DomainId { get; set; }
    [Required] public string Name { get; set; }
    [Required] public string Host { get; set; }
    [Required] public int Port { get; set; }
    [Required] public bool IsSecure { get; set; }
    [Required] public string Username { get; set; }
    public DateTime? Disabled { get; set; }
}