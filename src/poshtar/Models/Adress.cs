using poshtar.Entities;

namespace poshtar.Models;

public class AddressVM
{
    public int AddressId { get; set; }
    public int DomainId { get; set; }
    public string Pattern { get; set; }
    public string? Description { get; set; }
    public bool IsStatic { get; set; }
    public DateTime? Disabled { get; set; }
    public DomainVM? Domain { get; set; }
    public List<UserVM>? Users { get; set; }

    public AddressVM(Address e, bool loadRelatedEntities = true)
    {
        AddressId = e.AddressId;
        DomainId = e.DomainId;
        Pattern = e.Pattern;
        Description = e.Description;
        IsStatic = e.IsStatic;
        Disabled = e.Disabled;

        if (!loadRelatedEntities)
            return;

        if (e.Domain != null)
            Domain = new(e.Domain);

        if (e.Users.Count > 0)
        {
            Users = new(e.Users.Count);
            foreach (var user in e.Users)
                Users.Add(new(user));
        }
    }
}