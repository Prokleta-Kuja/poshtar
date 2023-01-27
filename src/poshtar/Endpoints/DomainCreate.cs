using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;

namespace poshtar.Endpoints;

public class CreateDomain : IEndpointRequest<DomainCreateResponse>
{
    public required string Name { get; set; }
    public required string Host { get; set; }
    public int Port { get; set; }
    public bool IsSecure { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }

    public async Task<DomainCreateResponse> HandleAsync(IServiceProvider sp)
    {
        var logger = sp.GetRequiredService<ILogger<CreateDomain>>();
        var db = sp.GetRequiredService<AppDbContext>();

        Name = Name.Trim().ToLower();
        var isDuplicate = await db.Domains
            .AsNoTracking()
            .Where(x => x.Name == Name)
            .AnyAsync();

        if (isDuplicate)
            throw new ParamException(nameof(Name), "Already exists");

        var domain = new Domain
        {
            Name = Name,
            Host = Host,
            Port = Port,
            IsSecure = IsSecure,
            Username = Username,
            Password = Password,
        };

        db.Domains.Add(domain);
        await db.SaveChangesAsync();

        var response = new DomainCreateResponse { DomainId = domain.DomainId };
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

        if (string.IsNullOrWhiteSpace(Password))
            errors.Add(nameof(Password), "Required");

        return errors;
    }
}
public record DomainCreateResponse
{
    [Required] public int DomainId { get; set; }
}