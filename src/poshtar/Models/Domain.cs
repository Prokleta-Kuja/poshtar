using poshtar.Entities;

namespace poshtar.Models;

public class DomainVM
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

    public DomainVM(Domain e, bool loadRelatedEntities = true)
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