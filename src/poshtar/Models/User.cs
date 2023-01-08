using poshtar.Entities;

namespace poshtar.Models;

public class UserVM
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsMaster { get; set; }
    public int? Quota { get; set; }
    public DateTime? Disabled { get; set; }

    public List<DomainVM>? Domains { get; set; }
    public List<AddressVM>? Addresses { get; set; }

    public UserVM(User e, bool loadRelatedEntities = true)
    {
        UserId = e.UserId;
        Name = e.Name;
        Description = e.Description;
        IsMaster = e.IsMaster;
        Quota = e.Quota;
        Disabled = e.Disabled;

        if (!loadRelatedEntities)
            return;

        if (e.Domains.Count > 0)
        {
            Domains = new(e.Domains.Count);
            foreach (var domain in e.Domains)
                Domains.Add(new(domain));
        }

        if (e.Addresses.Count > 0)
        {
            Addresses = new(e.Addresses.Count);
            foreach (var address in e.Addresses)
                Addresses.Add(new(address));
        }
    }
}