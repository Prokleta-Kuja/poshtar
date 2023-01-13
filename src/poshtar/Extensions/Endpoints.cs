using poshtar.Extensions;
using poshtar.Requests;

namespace poshtar.Endpoints;

public static class EndpointExtensions
{
    public static void MapApi(this RouteGroupBuilder group)
    {
        //group.RequireAuthorization();
        group.MediateValidGet<Domains>("/domains");
        group.MediateValidGet<DomainById>("/domains/{id:int}");
        group.RequestValidGet<MyDomainById, MyDomainVM>("/domains2/{id:int}");
    }
}