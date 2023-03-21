using Microsoft.AspNetCore.Mvc;
using poshtar.Models;
using poshtar.Services;

namespace poshtar.Controllers;

[ApiController]
[Route("api/services")]
[Tags("services")]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
public class ServicesController : ControllerBase
{
    readonly ILogger<ServicesController> _logger;

    public ServicesController(ILogger<ServicesController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "Request")]
    [ProducesResponseType(typeof(AuthStatusModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Status(ServiceRequestModel model)
    {
        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var result = (model.Name, model.Type) switch
        {
            (ServiceName.Dovecot, ServiceRequestType.Status) => await BashExec.StatusDovecotAsync(),
            (ServiceName.Dovecot, ServiceRequestType.Start) => await BashExec.StartDovecotAsync(),
            (ServiceName.Dovecot, ServiceRequestType.Restart) => await BashExec.RestartDovecotAsync(),
            (ServiceName.Dovecot, ServiceRequestType.Stop) => await BashExec.StopDovecotAsync(),
            _ => (exitCode: -1, error: string.Empty, output: string.Empty)
        };

        return Ok(new ServiceResultModel(result));
    }
}

public enum ServiceName
{
    None = 0,
    Dovecot = 1,
}

public enum ServiceRequestType
{
    Nothing = 0,
    Status = 1,
    Start = 2,
    Restart = 3,
    Stop = 4,
}