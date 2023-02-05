using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Models;

namespace poshtar.Controllers;

// [Authorize]
[ApiController]
[Route("api/[controller]")]
[Tags(nameof(Entities.Address))]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(ValidationError))]
public class AddressesController : ControllerBase
{
    readonly ILogger<AddressesController> _logger;
    readonly AppDbContext _db;

    public AddressesController(ILogger<AddressesController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet(Name = "GetAddresses")]
    [ProducesResponseType(typeof(ListResponse<AddressLM>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] AddressQuery req)
    {

        var query = _db.Addresses.Include(a => a.Domain).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            query = query.Where(a => EF.Functions.Like(a.Pattern, $"%{req.SearchTerm}%") || EF.Functions.Like(a.Description!, $"%{req.SearchTerm}%"));

        if (req.DomainId.HasValue)
            query = query.Where(a => a.DomainId == req.DomainId.Value);
        else if (req.NotDomainId.HasValue)
            query = query.Where(a => a.DomainId != req.NotDomainId.Value);

        if (req.UserId.HasValue)
            query = query.Where(a => a.Users.Any(u => u.UserId == req.UserId.Value));
        else if (req.NotUserId.HasValue)
            query = query.Where(a => a.Users.Any(u => u.UserId != req.NotUserId.Value));

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(req.SortBy) && Enum.TryParse<AddressesSortBy>(req.SortBy, true, out var sortBy))
            query = sortBy switch
            {
                AddressesSortBy.Pattern => query.Order(a => a.Pattern, req.Ascending),
                AddressesSortBy.Description => query.Order(a => a.Description, req.Ascending),
                AddressesSortBy.IsStatic => query.Order(a => a.IsStatic, req.Ascending),
                AddressesSortBy.Domain => query.Order(a => a.Domain!.Name, req.Ascending),
                _ => query
            };

        var items = await query
            .Paginate(req)
            .Select(a => new AddressLM
            {
                Id = a.AddressId,
                DomainId = a.DomainId,
                DomainName = a.Domain!.Name,
                Pattern = a.Pattern,
                Description = a.Description,
                IsStatic = a.IsStatic,
                Disabled = a.Disabled,
                UserCount = a.Users.Count,
            })
            .ToListAsync();

        return Ok(new ListResponse<AddressLM>(req, count, items));
    }

    [HttpGet("{id}", Name = "GetAddress")]
    [ProducesResponseType(typeof(AddressVM), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOneAsnyc(int id)
    {
        var address = await _db.Addresses
           .Where(a => a.AddressId == id)
           .Select(a => new AddressVM(a))
           .FirstOrDefaultAsync();

        if (address == null)
            return NotFound();

        return Ok(address);
    }

    [HttpPost(Name = "CreateAddress")]
    [ProducesResponseType(typeof(AddressVM), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(AddressCM model)
    {
        if (model.IsStatic)
            model.Pattern = model.Pattern.Trim().ToLower();

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Addresses
            .AsNoTracking()
            .Where(a => a.Pattern == model.Pattern && a.IsStatic == model.IsStatic)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.Pattern), "Already exists"));

        var domain = await _db.Domains.FirstOrDefaultAsync(d => d.DomainId == model.DomainId);
        if (domain == null)
            return BadRequest(new ValidationError(nameof(model.DomainId), "Not found"));

        var address = new Address
        {
            Pattern = model.Pattern,
            Description = model.Description,
            IsStatic = model.IsStatic,
        };

        _db.Addresses.Add(address);
        await _db.SaveChangesAsync();

        return Ok(new AddressVM(address));
    }

    [HttpPut("{id}", Name = "UpdateAddress")]
    [ProducesResponseType(typeof(AddressVM), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, AddressUM model)
    {
        var address = await _db.Addresses
          .Where(a => a.AddressId == id)
          .FirstOrDefaultAsync();

        if (address == null)
            return NotFound();

        if (model.IsStatic)
            model.Pattern = model.Pattern.Trim().ToLower();

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Addresses
            .AsNoTracking()
            .Where(a => a.AddressId != id && a.Pattern == model.Pattern && a.IsStatic == model.IsStatic)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.Pattern), "Already exists"));

        var domain = await _db.Domains.FirstOrDefaultAsync(d => d.DomainId == model.DomainId);
        if (domain == null)
            return BadRequest(new ValidationError(nameof(model.DomainId), "Not found"));

        address.Pattern = model.Pattern;
        address.Description = model.Description;
        address.IsStatic = model.IsStatic;
        if (model.Disabled.HasValue)
            address.Disabled = model.Disabled.Value ? address.Disabled.HasValue ? address.Disabled : DateTime.UtcNow : null;

        await _db.SaveChangesAsync();

        return Ok(new AddressVM(address));
    }

    [HttpDelete("{id}", Name = "DeleteAddress")]
    [ProducesResponseType(typeof(AddressVM), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var address = await _db.Addresses
          .Where(d => d.AddressId == id)
          .FirstOrDefaultAsync();

        if (address == null)
            return NotFound();

        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public class AddressQuery : FilterQuery
{
    public int? DomainId { get; set; }
    public int? NotDomainId { get; set; }
    public int? UserId { get; set; }
    public int? NotUserId { get; set; }
}

public enum AddressesSortBy
{
    Pattern = 0,
    Description = 1,
    IsStatic = 2,
    Domain = 3,
}