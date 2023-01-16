using System.ComponentModel.DataAnnotations;
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
        group.MapEndpointGet<DomainById, DomainByIdResult>("/domains/{id:int}").WithName(nameof(DomainById)).WithTags("Domain");
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
    static IResult BadRequest(string message, IDictionary<string, string[]>? errors = null)
        => TypedResults.BadRequest(new BadResponse(message, errors));
}
public interface IEndpointRequest<TResponse>
{
    Task<TResponse> HandleAsync(IServiceProvider sp);
    Dictionary<string, string[]> Validate(IServiceProvider sp)
    {
        return new(0);
    }
}
public class BadResponse
{
    public BadResponse(string message, IDictionary<string, string[]>? errors = null)
    {
        ErrorMessage = message;
        Errors = errors;
    }

    [Required] public string ErrorMessage { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
}

#region Exceptions
public class ParamException : Exception
{
    public Dictionary<string, string[]> Errors { get; set; } = new();
    public ParamException(string param, params string[] errors)
    {
        Errors.Add(param, errors);
    }
    public void AddError(string param, params string[] errors)
    {
        Errors.Add(param, errors);
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