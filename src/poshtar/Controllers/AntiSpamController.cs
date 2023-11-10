using Microsoft.AspNetCore.Mvc;
using poshtar.Models;
using poshtar.Smtp;

namespace poshtar.Controllers;

[ApiController]
[Route("api/antispam")]
[Tags("AntiSpam")]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
public class AntiSpamController : ControllerBase
{
    readonly ILogger<AntiSpamController> _logger;
    public AntiSpamController(ILogger<AntiSpamController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetAntiSpam")]
    [ProducesResponseType(typeof(AntiSpamSettings), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(C.Smtp.AntiSpamSettings);
    }

    [HttpPut(Name = "UpdateAntiSpam")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutAsync(AntiSpamSettings model)
    {
        if (model.BanMinutes < 0)
            model.BanMinutes = 0;

        if (model.TarpitSeconds < 0)
            model.TarpitSeconds = 0;

        if (model.ConsecutiveCmdFail < 0)
            model.ConsecutiveCmdFail = 0;

        if (model.ConsecutiveRcptFail < 0)
            model.ConsecutiveRcptFail = 0;

        C.Smtp.AntiSpamSettings = model;
        await C.Settings.WriteAntiSpamAsync(C.Smtp.AntiSpamSettings);

        return Ok();
    }
}