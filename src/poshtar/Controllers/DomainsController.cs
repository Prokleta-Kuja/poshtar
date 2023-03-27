using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Models;

namespace poshtar.Controllers;

[ApiController]
[Route("api/domains")]
[Tags(nameof(Domain))]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
public class DomainsController : ControllerBase
{
    readonly ILogger<DomainsController> _logger;
    readonly AppDbContext _db;
    readonly IDataProtectionProvider _dpp;

    public DomainsController(ILogger<DomainsController> logger, AppDbContext db, IDataProtectionProvider dpp)
    {
        _logger = logger;
        _db = db;
        _dpp = dpp;
    }

    [HttpGet(Name = "GetDomains")]
    [ProducesResponseType(typeof(ListResponse<DomainLM>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] DomainQuery req)
    {
        var query = _db.Domains.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            query = query.Where(d => EF.Functions.Like(d.Name, $"%{req.SearchTerm}%") || EF.Functions.Like(d.Host, $"%{req.SearchTerm}%"));

        if (req.AddressId.HasValue)
            query = query.Where(u => u.Addresses.Any(a => a.AddressId == req.AddressId.Value));
        else if (req.NotAddressId.HasValue)
            query = query.Where(u => u.Addresses.Any(a => a.AddressId != req.NotAddressId.Value));

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(req.SortBy) && Enum.TryParse<DomainsSortBy>(req.SortBy, true, out var sortBy))
            query = sortBy switch
            {
                DomainsSortBy.Name => query.Order(d => d.Name, req.Ascending),
                DomainsSortBy.Host => query.Order(d => d.Host, req.Ascending),
                DomainsSortBy.Username => query.Order(d => d.Username, req.Ascending),
                DomainsSortBy.Port => query.Order(d => d.Port, req.Ascending),
                DomainsSortBy.Disabled => query.Order(d => d.Disabled, req.Ascending),
                DomainsSortBy.AddressCount => query.Order(d => d.Addresses.Count(), req.Ascending),
                _ => query
            };

        var items = await query
            .Paginate(req)
            .Select(d => new DomainLM
            {
                Id = d.DomainId,
                Name = d.Name,
                Host = d.Host,
                Username = d.Username,
                Port = d.Port,
                Disabled = d.Disabled,
                AddressCount = d.Addresses.Count,
            })
            .ToListAsync();
        return Ok(new ListResponse<DomainLM>(req, count, items));
    }

    [HttpGet("{domainId}", Name = "GetDomain")]
    [ProducesResponseType(typeof(DomainVM), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOneAsnyc(int domainId)
    {
        var domain = await _db.Domains
           .AsNoTracking()
           .Where(d => d.DomainId == domainId)
           .Select(d => new DomainVM(d))
           .FirstOrDefaultAsync();

        if (domain == null)
            return NotFound(new PlainError("Not found"));

        return Ok(domain);
    }

    [HttpPost(Name = "CreateDomain")]
    [ProducesResponseType(typeof(DomainVM), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(DomainCM model)
    {
        model.Name = model.Name.Trim().ToLower();
        model.Host = model.Host.Trim().ToLower();

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Domains
            .AsNoTracking()
            .Where(d => d.Name == model.Name)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.Name), "Already exists"));

        var serverProtector = _dpp.CreateProtector(nameof(Domain));

        var domain = new Domain
        {
            Name = model.Name,
            Host = model.Host,
            Port = model.Port,
            Username = model.Username,
            Password = serverProtector.Protect(model.Password),
        };

        _db.Domains.Add(domain);
        await _db.SaveChangesAsync();

        return Ok(new DomainVM(domain));
    }

    [HttpPut("{domainId}", Name = "UpdateDomain")]
    [ProducesResponseType(typeof(DomainVM), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int domainId, DomainUM model)
    {
        var domain = await _db.Domains
          .Where(d => d.DomainId == domainId)
          .FirstOrDefaultAsync();

        if (domain == null)
            return NotFound(new PlainError("Not found"));

        model.Name = model.Name.Trim().ToLower();
        model.Host = model.Host.Trim().ToLower();

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Domains
            .AsNoTracking()
            .Where(d => d.DomainId != domain.DomainId && d.Name == model.Name)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.Name), "Already exists"));

        domain.Name = model.Name;
        domain.Host = model.Host;
        domain.Port = model.Port;
        domain.Username = model.Username;
        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            var serverProtector = _dpp.CreateProtector(nameof(Domain));
            domain.Password = serverProtector.Protect(model.NewPassword);
        }
        if (model.Disabled.HasValue)
            domain.Disabled = model.Disabled.Value ? domain.Disabled.HasValue ? domain.Disabled : DateTime.UtcNow : null;

        await _db.SaveChangesAsync();

        return Ok(new DomainVM(domain));
    }

    [HttpDelete("{domainId}", Name = "DeleteDomain")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int domainId)
    {
        var domain = await _db.Domains
          .Where(d => d.DomainId == domainId)
          .FirstOrDefaultAsync();

        if (domain == null)
            return NotFound(new PlainError("Not found"));

        _db.Domains.Remove(domain);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public class DomainQuery : FilterQuery
{
    public int? AddressId { get; set; }
    public int? NotAddressId { get; set; }
    public int? UserId { get; set; }
    public int? NotUserId { get; set; }
}

public enum DomainsSortBy
{
    Name = 0,
    Host = 1,
    Username = 2,
    Port = 3,
    Disabled = 4,
    AddressCount = 5,
}