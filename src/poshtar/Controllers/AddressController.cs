using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Models;

namespace poshtar.Controllers;

// [Authorize]
[ApiController]
[Route("api/addresses")]
[Tags(nameof(Address))]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
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

        if (req.UserId.HasValue)
            query = query.Where(a => a.Users.Any(u => u.UserId == req.UserId.Value));
        else if (req.NotUserId.HasValue)
            query = query.Where(a => !a.Users.Any(u => u.UserId == req.NotUserId.Value));

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(req.SortBy) && Enum.TryParse<AddressesSortBy>(req.SortBy, true, out var sortBy))
            query = sortBy switch
            {
                AddressesSortBy.Pattern => query.Order(a => a.Pattern, req.Ascending),
                AddressesSortBy.Description => query.Order(a => a.Description, req.Ascending),
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
                Type = a.Type,
                Disabled = a.Disabled,
                UserCount = a.Users.Count,
            })
            .ToListAsync();

        return Ok(new ListResponse<AddressLM>(req, count, items));
    }

    [HttpGet("{addressId}", Name = "GetAddress")]
    [ProducesResponseType(typeof(AddressVM), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOneAsnyc(int addressId)
    {
        var address = await _db.Addresses
           .Where(a => a.AddressId == addressId)
           .Select(a => new AddressVM(a))
           .FirstOrDefaultAsync();

        if (address == null)
            return NotFound(new PlainError("Not found"));

        return Ok(address);
    }

    [HttpPost(Name = "CreateAddress")]
    [ProducesResponseType(typeof(AddressVM), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(AddressCM model)
    {
        if (model.Type == AddressType.CatchAll)
            model.Pattern = "*";
        else
            model.Pattern = model.Pattern.Trim().ToLower();

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var domain = await _db.Domains.FirstOrDefaultAsync(d => d.DomainId == model.DomainId);
        if (domain == null)
            return BadRequest(new ValidationError(nameof(model.DomainId), "Not found"));

        var isDuplicate = await _db.Addresses
            .AsNoTracking()
            .Where(a => a.DomainId == model.DomainId && a.Pattern == model.Pattern && a.Type == model.Type)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.Pattern), "Already exists"));

        var address = new Address
        {
            Pattern = model.Pattern,
            Description = model.Description,
            Type = model.Type,
        };

        domain.Addresses.Add(address);
        await _db.SaveChangesAsync();

        return Ok(new AddressVM(address));
    }

    [HttpPut("{addressId}", Name = "UpdateAddress")]
    [ProducesResponseType(typeof(AddressVM), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int addressId, AddressUM model)
    {
        var address = await _db.Addresses
          .Where(a => a.AddressId == addressId)
          .FirstOrDefaultAsync();

        if (address == null)
            return NotFound(new PlainError("Not found"));

        if (model.Type == AddressType.CatchAll)
            model.Pattern = "*";
        else
            model.Pattern = model.Pattern.Trim().ToLower();

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Addresses
            .AsNoTracking()
            .Where(a => a.AddressId != addressId && a.DomainId == model.DomainId && a.Pattern == model.Pattern && a.Type == model.Type)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.Pattern), "Already exists"));

        var domain = await _db.Domains.FirstOrDefaultAsync(d => d.DomainId == model.DomainId);
        if (domain == null)
            return BadRequest(new ValidationError(nameof(model.DomainId), "Not found"));

        address.Pattern = model.Pattern;
        address.Description = model.Description;
        address.Type = model.Type;
        if (model.Disabled.HasValue)
            address.Disabled = model.Disabled.Value ? address.Disabled.HasValue ? address.Disabled : DateTime.UtcNow : null;

        await _db.SaveChangesAsync();

        return Ok(new AddressVM(address));
    }

    [HttpDelete("{addressId}", Name = "DeleteAddress")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int addressId)
    {
        var address = await _db.Addresses
          .Where(d => d.AddressId == addressId)
          .FirstOrDefaultAsync();

        if (address == null)
            return NotFound(new PlainError("Not found"));

        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{addressId}/users/{userId}", Name = "AddAddressUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAddressUserAsync(int addressId, int userId)
    {
        var address = await _db.Addresses
            .Include(a => a.Users.Where(u => u.UserId == userId))
            .Where(a => a.AddressId == addressId)
            .FirstOrDefaultAsync();

        if (address == null)
            return NotFound(new PlainError("Address not found"));

        if (address.Users.Count > 0)
            return Conflict(new PlainError("Address already contains user"));

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return NotFound(new PlainError("User not found"));

        address.Users.Add(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{addressId}/users/{userId}", Name = "RemoveAddressUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveAddressUserAsync(int addressId, int userId)
    {
        var address = await _db.Addresses
            .Include(a => a.Users.Where(u => u.UserId == userId))
            .Where(a => a.AddressId == addressId)
            .FirstOrDefaultAsync();

        if (address == null)
            return NotFound(new PlainError("Address not found"));

        if (address.Users.Count == 0)
            return Conflict(new PlainError("User already removed from address"));

        address.Users.RemoveAt(0);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public class AddressQuery : FilterQuery
{
    public int? DomainId { get; set; }
    public int? UserId { get; set; }
    public int? NotUserId { get; set; }
}

public enum AddressesSortBy
{
    Pattern = 0,
    Description = 1,
    Domain = 2,
}