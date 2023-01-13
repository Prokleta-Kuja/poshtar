using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace poshtar.Extensions;

public interface IMyRequest<TResponse>
{
    Task<TResponse> HandleAsync(IServiceProvider sp);
    Dictionary<string, string[]> Validate(IServiceProvider sp);
}

internal static class RequestExtensions
{
    internal static void RequestValidGet<TRequest, TResponse>(this RouteGroupBuilder group, string template) where TRequest : IMyRequest<TResponse>
        => group.MapGet(template, RequestAndValidateAsync<TRequest, TResponse>())
            .Produces<TResponse>()
            .Produces<BadResponse>((int)HttpStatusCode.BadRequest);
    internal static void RequestValidPost<TRequest, TResponse>(this RouteGroupBuilder group, string template) where TRequest : IMyRequest<TResponse>
        => group.MapPost(template, RequestAndValidateAsync<TRequest, TResponse>());
    internal static void RequestValidPut<TRequest, TResponse>(this RouteGroupBuilder group, string template) where TRequest : IMyRequest<TResponse>
         => group.MapPut(template, RequestAndValidateAsync<TRequest, TResponse>());
    internal static void RequestValidDelete<TRequest, TResponse>(this RouteGroupBuilder group, string template) where TRequest : IMyRequest<TResponse>
        => group.MapDelete(template, RequestAndValidateAsync<TRequest, TResponse>());
    static Func<IServiceProvider, TRequest, Task<IResult>> RequestAndValidateAsync<TRequest, TResponse>() where TRequest : IMyRequest<TResponse>
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
            catch (Exception ex)
            {
                Log.Fatal(ex, "Couldn't complete request");
                return TypedResults.StatusCode((int)HttpStatusCode.InternalServerError);
            }
        };
    }
    static IResult BadRequest(string message, IDictionary<string, string[]>? errors = null)
        => TypedResults.BadRequest(new BadResponse(message, errors));
}
public class BadResponse
{
    public BadResponse(string message, IDictionary<string, string[]>? errors = null)
    {
        ErrorMessage = message;
        Errors = errors;
    }

    public string ErrorMessage { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
}

// public class ParamException : Exception
// {
//     public Dictionary<string, string[]> Errors { get; set; } = new();
//     public ParamException(string param, params string[] errors)
//     {
//         Errors.Add(param, errors);
//     }
//     public void AddError(string param, params string[] errors)
//     {
//         Errors.Add(param, errors);
//     }
// }