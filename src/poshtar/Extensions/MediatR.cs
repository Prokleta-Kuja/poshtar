using System.Net;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace poshtar.Extensions;

internal static class MediatRExtensions
{
    internal static void MediateValidGet<TRequest>(this RouteGroupBuilder group, string template) => group.MapGet(template, MediateAndValidateAsync<TRequest>());
    internal static void MediateValidPost<TRequest>(this RouteGroupBuilder group, string template) => group.MapPost(template, MediateAndValidateAsync<TRequest>());
    internal static void MediateValidPut<TRequest>(this RouteGroupBuilder group, string template) => group.MapPut(template, MediateAndValidateAsync<TRequest>());
    internal static void MediateValidDelete<TRequest>(this RouteGroupBuilder group, string template) => group.MapDelete(template, MediateAndValidateAsync<TRequest>());
    static Func<IMediator, IValidator<TRequest>, TRequest, Task<IResult>> MediateAndValidateAsync<TRequest>()
    {
        return async ([FromServices] IMediator mediator, [FromServices] IValidator<TRequest> validator, [AsParameters] TRequest request) =>
        {
            try
            {
                if (request == null)
                    return BadRequest("Could not parse request");

                var validationResult = validator.Validate(request);
                if (validationResult.IsValid)
                {
                    var result = await mediator.Send(request);
                    return Results.Ok(result);
                }

                return BadRequest("Validation errors", validationResult.ToDictionary());
            }
            catch (ParamException pe)
            {
                return BadRequest("Validation errors", pe.Errors);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Couldn't complete MediatR request");
                return Results.StatusCode((int)HttpStatusCode.InternalServerError);
            }
        };
    }
    static IResult BadRequest(string message, IDictionary<string, string[]>? errors = null)
        => Results.BadRequest(new { message, errors });
}

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