using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Models;

namespace poshtar.Controllers;

// [Authorize]
[ApiController]
[Route("api/log-entires")]
[Tags(nameof(Entities.LogEntry))]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
public class LogEntriesController : ControllerBase
{
    readonly ILogger<LogEntriesController> _logger;
    readonly AppDbContext _db;

    public LogEntriesController(ILogger<LogEntriesController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }
    [HttpGet(Name = "GetLogEntries")]
    [ProducesResponseType(typeof(ListResponse<LogEntryVM>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] LogEntryQuery req)
    {
        var query = _db.Logs.AsNoTracking();

        if (req.Context.HasValue && req.Context.Value != Guid.Empty)
            query = query.Where(le => le.Context == req.Context.Value);
        else
        {
            if (!string.IsNullOrWhiteSpace(req.SearchTerm))
                query = query.Where(le => EF.Functions.Like(le.Message, $"%{req.SearchTerm}%") || EF.Functions.Like(le.Properties!, $"%{req.SearchTerm}%"));
            if (req.From.HasValue)
                query = query.Where(le => le.Timestamp >= req.From.Value);
            if (req.Till.HasValue)
                query = query.Where(le => le.Timestamp <= req.Till.Value);
        }
        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(req.SortBy) && Enum.TryParse<LogEntrySortBy>(req.SortBy, true, out var sortBy))
            query = sortBy switch
            {
                LogEntrySortBy.Timestamp => query.Order(le => le.Timestamp, req.Ascending),
                _ => query.OrderByDescending(le => le.Timestamp)
            };

        var items = await query
            .Paginate(req)
            .Select(le => new LogEntryVM(le))
            .ToListAsync();
        return Ok(new ListResponse<LogEntryVM>(req, count, items));
    }
}

public class LogEntryQuery : FilterQuery
{
    public Guid? Context { get; set; }
    public DateTime? From { get; set; }
    public DateTime? Till { get; set; }
}

public enum LogEntrySortBy
{
    Timestamp = 0,
}