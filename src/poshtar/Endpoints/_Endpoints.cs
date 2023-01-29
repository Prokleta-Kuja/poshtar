using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace poshtar.Endpoints;

public static class Endpoints
{
    public static void MapApi(this RouteGroupBuilder group)
    {
        //group.RequireAuthorization()
        var domain = nameof(Entities.Domain);
        group.MapEndpointGet<GetDomains, Response<Domains>>("/domains").WithName(nameof(GetDomains)).WithTags(domain);
        group.MapEndpointPost<CreateDomain, DomainCreateResponse>("/domains").WithName(nameof(CreateDomain)).WithTags(domain);
        group.MapEndpointGet<GetDomainById, DomainByIdResponse>("/domains/{id:int}").WithName(nameof(GetDomainById)).WithTags(domain);
        group.MapEndpointPut<UpdateDomain, DomainUpdateResponse>("/domains/{id:int}").WithName(nameof(UpdateDomain)).WithTags(domain);

        var user = nameof(Entities.User);
        group.MapEndpointGet<GetUsers, Response<Users>>("/users").WithName(nameof(GetUsers)).WithTags(user);
        group.MapEndpointPost<CreateUser, UserCreateResponse>("/users").WithName(nameof(CreateUser)).WithTags(user);
        group.MapEndpointGet<GetUserById, UserByIdResponse>("/users/{id:int}").WithName(nameof(GetUserById)).WithTags(user);
        group.MapEndpointPut<UpdateUser, UserUpdateResponse>("/users/{id:int}").WithName(nameof(UpdateUser)).WithTags(user);

        var address = nameof(Entities.Address);
        group.MapEndpointGet<GetAddresses, Response<Addresses>>("/addresses").WithName(nameof(GetAddresses)).WithTags(address);
        group.MapEndpointPost<CreateAddress, AddressCreateResponse>("/addresses").WithName(nameof(CreateAddress)).WithTags(address);
        group.MapEndpointGet<GetAddressById, AddressByIdResponse>("/addresses/{id:int}").WithName(nameof(GetAddressById)).WithTags(address);
        group.MapEndpointPut<UpdateAddress, AddressUpdateResponse>("/addresses/{id:int}").WithName(nameof(UpdateAddress)).WithTags(address);
    }

    internal static RouteHandlerBuilder MapEndpointGet<TRequest, TResponse>(this RouteGroupBuilder group, string template) where TRequest : IEndpointRequest<TResponse>
       => group.MapGet(template, HandleEndpointRequestAsync<TRequest, TResponse>())
           .Produces<TResponse>()
           .Produces<BadResponse>(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status403Forbidden)
           .Produces(StatusCodes.Status500InternalServerError);

    internal static RouteHandlerBuilder MapEndpointPost<TRequest, TResponse>(this RouteGroupBuilder group, string template) where TRequest : IEndpointRequest<TResponse>
        => group.MapPost(template, HandleEndpointRequestAsync<TRequest, TResponse>())
           .Produces<TResponse>()
           .Produces<BadResponse>(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status403Forbidden)
           .Produces(StatusCodes.Status500InternalServerError);

    internal static RouteHandlerBuilder MapEndpointPut<TRequest, TResponse>(this RouteGroupBuilder group, string template) where TRequest : IEndpointRequest<TResponse>
        => group.MapPut(template, HandleEndpointRequestAsync<TRequest, TResponse>())
           .Produces<TResponse>()
           .Produces<BadResponse>(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status403Forbidden)
           .Produces(StatusCodes.Status500InternalServerError);

    internal static RouteHandlerBuilder MapEndpointDelete<TRequest, TResponse>(this RouteGroupBuilder group, string template) where TRequest : IEndpointRequest<TResponse>
        => group.MapDelete(template, HandleEndpointRequestAsync<TRequest, TResponse>())
           .Produces<TResponse>()
           .Produces<BadResponse>(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status403Forbidden)
           .Produces(StatusCodes.Status500InternalServerError);

    static Func<IServiceProvider, TRequest, Task<IResult>> HandleEndpointRequestAsync<TRequest, TResponse>() where TRequest : IEndpointRequest<TResponse>
    {
        return async ([FromServices] IServiceProvider sp, [AsParameters] TRequest request) =>
        {
            try
            {
                if (request == null)
                    return BadRequest("Could not parse request");

                var errors = request.Validate(sp);
                if (errors.Count > 0)
                    return BadRequest("Validation errors", errors);

                var result = await request.HandleAsync(sp);
                return TypedResults.Ok(result);
            }
            catch (ParamException pe)
            {
                return BadRequest("Validation errors", pe.Errors);
            }
            catch (ForbiddenException)
            {
                return TypedResults.Forbid();
            }
            catch (NotFoundException)
            {
                return TypedResults.NotFound("Entity not found");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Couldn't complete request");
                return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
            }
        };
    }
    static IResult BadRequest(string message, IDictionary<string, string>? errors = null)
        => TypedResults.BadRequest(new BadResponse(message, errors));

    internal static IQueryable<T> Paginate<T>(this IQueryable<T> query, ListRequest request)
    {
        if (request.Page.HasValue && request.Size.HasValue)
            return query.Skip((request.Page.Value - 1) * request.Size.Value);

        return query;
    }
    internal static IOrderedQueryable<T> Order<T, TKey>(this IQueryable<T> source, Expression<Func<T, TKey>> selector, bool? ascending)
    {
        return ascending.HasValue && ascending.Value ? source.OrderBy(selector) : source.OrderByDescending(selector);
    }
}

#region Requests and responses
public interface IEndpointRequest<TResponse>
{
    Task<TResponse> HandleAsync(IServiceProvider sp);
    Dictionary<string, string> Validate(IServiceProvider sp)
    {
        return new(0);
    }
}

public class BadResponse
{
    public BadResponse(string message, IDictionary<string, string>? errors = null)
    {
        ErrorMessage = message;
        Errors = errors;
    }

    [Required] public string ErrorMessage { get; set; }
    public IDictionary<string, string>? Errors { get; set; }
}

public class ListRequest
{
    private int size;
    private int page;

    [FromQuery] public int? Size { get => size; set => size = value.HasValue ? value.Value > 100 ? 100 : value.Value < 1 ? 1 : value.Value : 25; }
    [FromQuery] public int? Page { get => page; set => page = value.HasValue ? value.Value <= 0 ? 1 : value.Value : 1; }
    [FromQuery] public bool? Ascending { get; set; }
    [FromQuery] public string? SortBy { get; set; }
}


public record Response<T>
{
    public Response(ListRequest req, int total, List<T> items)
    {
        Size = req.Size!.Value;
        Page = req.Page!.Value;
        Total = total;
        Items = items;
        Ascending = req.Ascending ?? false;
        SortBy = req.SortBy;
    }

    [Required] public List<T> Items { get; init; }
    [Required] public int Size { get; init; }
    [Required] public int Page { get; init; }
    [Required] public int Total { get; init; }
    public bool Ascending { get; init; }
    public string? SortBy { get; init; }
}
#endregion

#region Exceptions
public class ParamException : Exception
{
    public Dictionary<string, string> Errors { get; set; } = new();
    public ParamException(string param, string error)
    {
        Errors.Add(param, error);
    }
    public void AddError(string param, string error)
    {
        Errors.Add(param, error);
    }
}
public class ForbiddenException : Exception { }
public class NotFoundException : Exception { }
#endregion

#region OpenApi
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            var array = new OpenApiArray();
            array.AddRange(Enum.GetNames(context.Type).Select(n => new OpenApiString(n)));
            schema.Extensions.Add("x-enum-varnames", array);
        }
    }
}
#endregion