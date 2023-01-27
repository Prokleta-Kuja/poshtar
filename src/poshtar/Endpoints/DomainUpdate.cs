using System.ComponentModel.DataAnnotations;
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

    public async Task<DomainUpdateResponse> HandleAsync(IServiceProvider sp)
    {
        var logger = sp.GetRequiredService<ILogger<UpdateDomain>>();
        var db = sp.GetRequiredService<AppDbContext>();

        Name = Name.Trim().ToLower();
        var domain = await db.Domains.FirstOrDefaultAsync(x => x.DomainId == DomainId);

        if (domain == null)
            throw new NotFoundException();


        domain.Name = Name;
        domain.Host = Host;
        domain.Port = Port;
        domain.IsSecure = IsSecure;
        domain.Username = Username;
        if (!string.IsNullOrWhiteSpace(NewPassword))
            domain.Password = NewPassword;

        await db.SaveChangesAsync();

        var response = new DomainUpdateResponse(domain);
        return response;
    }

    public Dictionary<string, string> Validate(IServiceProvider sp)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add(nameof(Name), "Required");
        else if (!Uri.TryCreate(Name, UriKind.Absolute, out var full) || !string.IsNullOrWhiteSpace(full.PathAndQuery.Trim('/')))
            errors.Add(nameof(Name), "Invalid");

        if (string.IsNullOrWhiteSpace(Host))
            errors.Add(nameof(Host), "Required");
        else if (!Uri.TryCreate(Host, UriKind.Absolute, out var full) || !string.IsNullOrWhiteSpace(full.PathAndQuery.Trim('/')))
            errors.Add(nameof(Host), "Invalid");

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