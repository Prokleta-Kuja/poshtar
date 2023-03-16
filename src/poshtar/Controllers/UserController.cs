using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Models;
using poshtar.Services;

namespace poshtar.Controllers;

[ApiController]
[Route("api/users")]
[Tags(nameof(Entities.User))]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
public class UsersController : ControllerBase
{
    readonly ILogger<UsersController> _logger;
    readonly AppDbContext _db;
    readonly IDataProtectionProvider _dpp;

    public UsersController(ILogger<UsersController> logger, AppDbContext db, IDataProtectionProvider dpp)
    {
        _logger = logger;
        _db = db;
        _dpp = dpp;
    }

    [HttpGet(Name = "GetUsers")]
    [ProducesResponseType(typeof(ListResponse<UserLM>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] UserQuery req)
    {
        var query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            query = query.Where(u => u.Name.Contains(req.SearchTerm.ToLower()) || u.Description!.Contains(req.SearchTerm));

        if (req.AddressId.HasValue)
            query = query.Where(u => u.Addresses.Any(a => a.AddressId == req.AddressId.Value));
        else if (req.NotAddressId.HasValue)
            query = query.Where(u => u.Addresses.Any(a => a.AddressId != req.NotAddressId.Value));

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(req.SortBy) && Enum.TryParse<UsersSortBy>(req.SortBy, true, out var sortBy))
            query = sortBy switch
            {
                UsersSortBy.Name => query.Order(u => u.Name, req.Ascending),
                UsersSortBy.Description => query.Order(u => u.Description, req.Ascending),
                UsersSortBy.IsMaster => query.Order(u => u.IsMaster, req.Ascending),
                UsersSortBy.QuotaMegaBytes => query.Order(u => u.Quota == null, req.Ascending).Order(u => u.Quota, req.Ascending),
                UsersSortBy.AddressCount => query.Order(u => u.Addresses.Count, req.Ascending),
                UsersSortBy.Disabled => query.Order(u => u.Disabled, req.Ascending),
                _ => query
            };

        var items = await query
            .Paginate(req)
            .Select(u => new UserLM
            {
                Id = u.UserId,
                Name = u.Name,
                Description = u.Description,
                IsMaster = u.IsMaster,
                QuotaMegaBytes = u.Quota / 1024 / 1024,
                Disabled = u.Disabled,
                AddressCount = u.Addresses.Count,
            })
            .ToListAsync();

        return Ok(new ListResponse<UserLM>(req, count, items));
    }

    [HttpGet("{userId}", Name = "GetUser")]
    [ProducesResponseType(typeof(UserVM), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOneAsnyc(int userId)
    {
        var user = await _db.Users
           .Where(u => u.UserId == userId)
           .Select(u => new UserVM(u))
           .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new PlainError("Not found"));

        return Ok(user);
    }

    [HttpPost(Name = "CreateUser")]
    [ProducesResponseType(typeof(UserVM), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(UserCM model)
    {
        model.Name = model.Name.Trim().ToLower();

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Users
            .AsNoTracking()
            .Where(u => u.Name == model.Name)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.Name), "Already exists"));

        var result = DovecotHasher.Hash(model.Password);
        var user = new User
        {
            Name = model.Name,
            Description = model.Description,
            IsMaster = model.IsMaster,
            Quota = model.Quota * 1024 * 1024,
            Salt = result.Salt,
            Hash = result.Hash,
            Password = DovecotHasher.Password(result.Salt, result.Hash),
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new UserVM(user));
    }

    [HttpPut("{userId}", Name = "UpdateUser")]
    [ProducesResponseType(typeof(UserVM), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int userId, UserUM model)
    {
        var user = await _db.Users
          .Where(u => u.UserId == userId)
          .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new PlainError("Not found"));

        model.Name = model.Name.Trim().ToLower();

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Users
            .AsNoTracking()
            .Where(u => u.UserId != user.UserId && u.Name == model.Name)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.Name), "Already exists"));

        user.Name = model.Name;
        user.Description = model.Description;
        user.IsMaster = model.IsMaster;
        user.Quota = model.Quota * 1024 * 1024;
        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            var result = DovecotHasher.Hash(model.NewPassword);
            user.Salt = result.Salt;
            user.Hash = result.Hash;
            user.Password = DovecotHasher.Password(result.Salt, result.Hash);
        }
        if (model.Disabled.HasValue)
            user.Disabled = model.Disabled.Value ? user.Disabled.HasValue ? user.Disabled : DateTime.UtcNow : null;

        await _db.SaveChangesAsync();

        return Ok(new UserVM(user));
    }

    [HttpDelete("{userId}", Name = "DeleteUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int userId)
    {
        var user = await _db.Users
          .Where(u => u.UserId == userId)
          .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new PlainError("Not found"));

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public class UserQuery : FilterQuery
{
    public int? AddressId { get; set; }
    public int? NotAddressId { get; set; }
}

public enum UsersSortBy
{
    Name = 0,
    Description = 1,
    IsMaster = 2,
    QuotaMegaBytes = 3,
    AddressCount = 4,
    Disabled = 5,
}