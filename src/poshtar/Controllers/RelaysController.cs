using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Models;

namespace poshtar.Controllers;

[ApiController]
[Route("api/relays")]
[Tags(nameof(Relay))]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
public class RelaysController : ControllerBase
{
    readonly ILogger<RelaysController> _logger;
    readonly AppDbContext _db;
    readonly IDataProtectionProvider _dpp;

    public RelaysController(ILogger<RelaysController> logger, AppDbContext db, IDataProtectionProvider dpp)
    {
        _logger = logger;
        _db = db;
        _dpp = dpp;
    }

    [HttpGet(Name = "GetRelays")]
    [ProducesResponseType(typeof(ListResponse<RelayLM>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] FilterQuery req)
    {
        var query = _db.Relays.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            query = query.Where(d => EF.Functions.Like(d.Name, $"%{req.SearchTerm}%") || EF.Functions.Like(d.Host, $"%{req.SearchTerm}%"));

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(req.SortBy) && Enum.TryParse<RelaysSortBy>(req.SortBy, true, out var sortBy))
            query = sortBy switch
            {
                RelaysSortBy.Name => query.Order(r => r.Name, req.Ascending),
                RelaysSortBy.Host => query.Order(r => r.Host, req.Ascending),
                RelaysSortBy.Username => query.Order(r => r.Username, req.Ascending),
                RelaysSortBy.Port => query.Order(r => r.Port, req.Ascending),
                RelaysSortBy.Disabled => query.Order(r => r.Disabled, req.Ascending),
                RelaysSortBy.DomainCount => query.Order(r => r.Domains.Count(), req.Ascending),
                _ => query
            };

        var items = await query
            .Paginate(req)
            .Select(r => new RelayLM
            {
                Id = r.RelayId,
                Name = r.Name,
                Host = r.Host,
                Username = r.Username,
                Port = r.Port,
                Disabled = r.Disabled,
                DomainCount = r.Domains.Count,
            })
            .ToListAsync();
        return Ok(new ListResponse<RelayLM>(req, count, items));
    }

    [HttpGet("{relayId}", Name = "GetRelay")]
    [ProducesResponseType(typeof(RelayVM), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOneAsnyc(int relayId)
    {
        var relay = await _db.Relays
           .AsNoTracking()
           .Where(r => r.RelayId == relayId)
           .Select(r => new RelayVM(r))
           .FirstOrDefaultAsync();

        if (relay == null)
            return NotFound(new PlainError("Not found"));

        return Ok(relay);
    }

    [HttpPost(Name = "CreateRelay")]
    [ProducesResponseType(typeof(RelayVM), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(RelayCM model)
    {
        model.Host = model.Host.Trim().ToLower();

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Relays
            .AsNoTracking()
            .Where(r => r.Name == model.Name)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.Name), "Already exists"));

        var serverProtector = _dpp.CreateProtector(nameof(Relay));

        var relay = new Relay
        {
            Name = model.Name,
            Host = model.Host,
            Port = model.Port,
            Username = model.Username,
            Password = serverProtector.Protect(model.Password),
        };

        _db.Relays.Add(relay);
        await _db.SaveChangesAsync();

        return Ok(new RelayVM(relay));
    }

    [HttpPut("{relayId}", Name = "UpdateRelay")]
    [ProducesResponseType(typeof(RelayVM), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int relayId, RelayUM model)
    {
        var relay = await _db.Relays
          .Where(d => d.RelayId == relayId)
          .FirstOrDefaultAsync();

        if (relay == null)
            return NotFound(new PlainError("Not found"));

        model.Host = model.Host.Trim().ToLower();

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Relays
            .AsNoTracking()
            .Where(r => r.RelayId != relay.RelayId && r.Name == model.Name)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.Name), "Already exists"));

        relay.Name = model.Name;
        relay.Host = model.Host;
        relay.Port = model.Port;
        relay.Username = model.Username;
        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            var serverProtector = _dpp.CreateProtector(nameof(Relay));
            relay.Password = serverProtector.Protect(model.NewPassword);
        }
        if (model.Disabled.HasValue)
            relay.Disabled = model.Disabled.Value ? relay.Disabled.HasValue ? relay.Disabled : DateTime.UtcNow : null;

        await _db.SaveChangesAsync();

        return Ok(new RelayVM(relay));
    }

    [HttpDelete("{relayId}", Name = "DeleteRelay")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int relayId)
    {
        var relay = await _db.Relays
          .Where(d => d.RelayId == relayId)
          .FirstOrDefaultAsync();

        if (relay == null)
            return NotFound(new PlainError("Not found"));

        _db.Relays.Remove(relay);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public enum RelaysSortBy
{
    Name = 0,
    Host = 1,
    Username = 2,
    Port = 3,
    Disabled = 4,
    DomainCount = 5,
}