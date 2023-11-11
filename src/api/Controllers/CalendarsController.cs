using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Models;

namespace poshtar.Controllers;

[ApiController]
[Route("api/calendars")]
[Tags(nameof(Calendar))]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
public class CalendarsController : ControllerBase
{
    readonly ILogger<CalendarsController> _logger;
    readonly AppDbContext _db;

    public CalendarsController(ILogger<CalendarsController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet(Name = "GetCalendars")]
    [ProducesResponseType(typeof(ListResponse<CalendarLM>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] FilterQuery req)
    {
        var query = _db.Calendars.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            query = query.Where(c => c.DisplayName.Contains(req.SearchTerm));

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(req.SortBy) && Enum.TryParse<CalendarsSortBy>(req.SortBy, true, out var sortBy))
            query = sortBy switch
            {
                CalendarsSortBy.Name => query.Order(c => c.DisplayName, req.Ascending),
                _ => query
            };

        var items = await query
        .Paginate(req)
        .Select(c => new CalendarLM
        {
            Id = c.CalendarId,
            DisplayName = c.DisplayName,
            UserCount = c.CalendarUsers.Count,
            WriteUserCount = c.CalendarUsers.Where(cu => cu.CanWrite).Count(),
        })
        .ToListAsync();

        return Ok(new ListResponse<CalendarLM>(req, count, items));
    }

    [HttpGet("{calendarId}", Name = "GetCalendar")]
    [ProducesResponseType(typeof(CalendarVM), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOneAsnyc(int calendarId)
    {
        var calendar = await _db.Calendars
           .AsNoTracking()
           .Where(c => c.CalendarId == calendarId)
           .Include(c => c.CalendarUsers)
           .ThenInclude(cu => cu.User)
           .Select(c => new CalendarVM(c))
           .FirstOrDefaultAsync();

        if (calendar == null)
            return NotFound(new PlainError("Not found"));

        return Ok(calendar);
    }

    [HttpPost(Name = "CreateCalendar")]
    [ProducesResponseType(typeof(CalendarVM), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(CalendarCM model)
    {
        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Users
            .AsNoTracking()
            .Where(c => c.Name == model.DisplayName)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.DisplayName), "Already exists"));

        var calendar = new Calendar
        {
            DisplayName = model.DisplayName
        };

        _db.Calendars.Add(calendar);
        await _db.SaveChangesAsync();

        return Ok(new CalendarVM(calendar));
    }

    [HttpPut("{calendarId}", Name = "UpdateCalendar")]
    [ProducesResponseType(typeof(CalendarVM), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int calendarId, CalendarUM model)
    {
        var calendar = await _db.Calendars
          .Where(c => c.CalendarId == calendarId)
          .FirstOrDefaultAsync();

        if (calendar == null)
            return NotFound(new PlainError("Not found"));

        model.DisplayName = model.DisplayName;

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Calendars
            .AsNoTracking()
            .Where(c => c.CalendarId != calendar.CalendarId && c.DisplayName == model.DisplayName)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.DisplayName), "Already exists"));

        calendar.DisplayName = model.DisplayName;

        await _db.SaveChangesAsync();

        return Ok(new CalendarVM(calendar));
    }

    [HttpDelete("{calendarId}", Name = "DeleteCalendar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int calendarId)
    {
        var calendar = await _db.Calendars
          .Where(c => c.CalendarId == calendarId)
          .Include(c => c.CalendarObjects)
          .FirstOrDefaultAsync();

        if (calendar == null)
            return NotFound(new PlainError("Not found"));

        using var transaction = _db.Database.BeginTransaction();
        _db.Calendars.Remove(calendar);
        await _db.SaveChangesAsync();

        foreach (var obj in calendar.CalendarObjects)
            System.IO.File.Delete(C.Paths.CalendarObjectsDataFor(obj.FileName));

        await transaction.CommitAsync();

        return NoContent();
    }

    [HttpPost("{calendarId}/users/{userId}", Name = "AddCalendarUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddCalendarUserAsync(int calendarId, int userId, bool canWrite)
    {
        var calendar = await _db.Calendars
            .Include(c => c.CalendarUsers.Where(cu => cu.UserId == userId))
            .Where(c => c.CalendarId == calendarId)
            .FirstOrDefaultAsync();

        if (calendar == null)
            return NotFound(new PlainError("Calendar not found"));

        if (calendar.CalendarUsers.Count > 0)
            return Conflict(new PlainError("Calendar already contains user"));

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return NotFound(new PlainError("User not found"));

        calendar.CalendarUsers.Add(new CalendarUser
        {
            User = user,
            CanWrite = canWrite,
        });
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{calendarId}/users/{userId}", Name = "RemoveCalendarUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveCalendarUserAsync(int calendarId, int userId)
    {
        var calendar = await _db.Calendars
            .Include(c => c.CalendarUsers.Where(u => u.UserId == userId))
            .Where(c => c.CalendarId == calendarId)
            .FirstOrDefaultAsync();

        if (calendar == null)
            return NotFound(new PlainError("Calendar not found"));

        if (calendar.CalendarUsers.Count == 0)
            return Conflict(new PlainError("User already removed from calendar"));

        calendar.CalendarUsers.RemoveAt(0);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public enum CalendarsSortBy
{
    Name = 0,
}