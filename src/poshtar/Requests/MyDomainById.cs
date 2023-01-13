using MediatR;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Extensions;
using poshtar.Models;

namespace poshtar.Requests;

public class MyDomainById : IMyRequest<MyDomainVM>
{
    public int Id { get; set; }

    public async Task<MyDomainVM> HandleAsync(IServiceProvider sp)
    {
        var logger = sp.GetRequiredService<ILogger<MyDomainById>>();
        var db = sp.GetRequiredService<AppDbContext>();
        var domain = await db.Domains.FirstAsync();
        logger.LogInformation("OK");
        return new MyDomainVM(domain);
    }

    public Dictionary<string, string[]> Validate(IServiceProvider sp)
    {
        return new(0);
    }
}
public class MyDomainVM
{
    public int DomainId { get; set; }
    public string Name { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public bool IsSecure { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public DateTime? Disabled { get; set; }

    public List<AddressVM>? Addresses { get; set; }
    public List<UserVM>? Users { get; set; }

    public MyDomainVM(Domain e, bool loadRelatedEntities = true)
    {
        DomainId = e.DomainId;
        Name = e.Name;
        Host = e.Host;
        Port = e.Port;
        IsSecure = e.IsSecure;
        Username = e.Username;
        Password = e.Password;
        Disabled = e.Disabled;

        if (!loadRelatedEntities)
            return;

        if (e.Addresses.Count > 0)
        {
            Addresses = new(e.Addresses.Count);
            foreach (var address in e.Addresses)
                Addresses.Add(new(address));
        }

        if (e.Users.Count > 0)
        {
            Users = new(e.Users.Count);
            foreach (var user in e.Users)
                Users.Add(new(user));
        }
    }
}