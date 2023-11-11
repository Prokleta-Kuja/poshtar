using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Models;

namespace poshtar.Controllers;

[ApiController]
[Route("api/transactions")]
[Tags(nameof(Transaction))]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
public class TransactionsController : ControllerBase
{
    readonly ILogger<TransactionsController> _logger;
    readonly AppDbContext _db;

    public TransactionsController(ILogger<TransactionsController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet(Name = "GetTransactions")]
    [ProducesResponseType(typeof(ListResponse<TransactionLM>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] TransactionQuery req)
    {
        var query = _db.Transactions
            .Include(t => t.FromUser)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            query = query.Where(t => EF.Functions.Like(t.Client!, $"%{req.SearchTerm}%") || EF.Functions.Like(t.From!, $"%{req.SearchTerm}%"));

        if (req.ConnectionId.HasValue)
            query = query.Where(t => t.ConnectionId == req.ConnectionId);

        if (!req.IncludeMonitor.HasValue || !req.IncludeMonitor.Value)
            query = query.Where(t => t.CountryCode != C.COUNTRY_CODE_MONITOR);

        if (!req.IncludePrivate.HasValue || !req.IncludePrivate.Value)
            query = query.Where(t => t.CountryCode != C.COUNTRY_CODE_PRIVATE);

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(req.SortBy) && Enum.TryParse<TransactionsSortBy>(req.SortBy, true, out var sortBy))
            query = sortBy switch
            {
                TransactionsSortBy.Start => query.Order(t => t.Start, req.Ascending),
                TransactionsSortBy.CountryCode => query.Order(t => t.CountryCode, req.Ascending),
                TransactionsSortBy.Asn => query.Order(t => t.Asn, req.Ascending),
                TransactionsSortBy.Client => query.Order(t => t.Client, req.Ascending),
                TransactionsSortBy.From => query.Order(t => t.From, req.Ascending),
                _ => query
            };

        var items = await query
            .Paginate(req)
            .Select(t => new TransactionLM
            {
                Id = t.TransactionId,
                ConnectionId = t.ConnectionId,
                Submission = t.Submission,
                IpAddress = t.IpAddress,
                CountryCode = t.CountryCode,
                CountryName = t.CountryName,
                Asn = t.Asn,
                Start = t.Start,
                End = t.End,
                Client = t.Client,
                Username = t.FromUser != null ? t.FromUser.Name : string.Empty,
                From = t.From,
                Secure = t.Secure,
                Queued = t.Recipients.Any(),
            })
            .ToListAsync();
        return Ok(new ListResponse<TransactionLM>(req, count, items));
    }

    [HttpGet("{transactionId}/logs", Name = "GetLogs")]
    [ProducesResponseType(typeof(ListResponse<LogEntryLM>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogsAsnyc(int transactionId, [FromQuery] FilterQuery req)
    {
        var query = _db.Logs
            .Where(r => r.TransactionId == transactionId)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            query = query.Where(l => EF.Functions.Like(l.Message, $"%{req.SearchTerm}%") || EF.Functions.Like(l.Properties!, $"%{req.SearchTerm}%"));

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(req.SortBy) && Enum.TryParse<LogsSortBy>(req.SortBy, true, out var sortBy))
            query = sortBy switch
            {
                LogsSortBy.Timestamp => query.Order(l => l.Timestamp, req.Ascending),
                LogsSortBy.Message => query.Order(l => l.Message, req.Ascending),
                _ => query
            };

        var items = await query
            .Paginate(req)
            .Select(l => new LogEntryLM
            {
                Id = l.LogEntryId,
                Timestamp = l.Timestamp,
                Message = l.Message,
                Properties = l.Properties,
            })
            .ToListAsync();

        return Ok(new ListResponse<LogEntryLM>(req, count, items));
    }

    [HttpGet("{transactionId}/recipients", Name = "GetRecipients")]
    [ProducesResponseType(typeof(ListResponse<RecipientLM>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecipientsAsnyc(int transactionId, [FromQuery] FilterQuery req)
    {
        var query = _db.Recipients
            .Where(r => r.TransactionId == transactionId)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            query = query.Where(r => EF.Functions.Like(r.User!.Name, $"%{req.SearchTerm}%") || EF.Functions.Like(r.Data!, $"%{req.SearchTerm}%"));

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(req.SortBy) && Enum.TryParse<RecipientsSortBy>(req.SortBy, true, out var sortBy))
            query = sortBy switch
            {
                RecipientsSortBy.Username => query.Order(l => l.User!.Name, req.Ascending),
                RecipientsSortBy.Data => query.Order(l => l.Data, req.Ascending),
                _ => query
            };

        var items = await query
            .Paginate(req)
            .Select(r => new RecipientLM
            {
                Id = r.RecipientId,
                UserId = r.UserId,
                Data = r.Data,
            })
            .ToListAsync();

        return Ok(new ListResponse<RecipientLM>(req, count, items));
    }

    [HttpDelete("{transactionId}", Name = "DeleteTransaction")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int transactionId)
    {
        var transaction = await _db.Transactions
          .Where(d => d.TransactionId == transactionId)
          .FirstOrDefaultAsync();

        if (transaction == null)
            return NotFound(new PlainError("Not found"));

        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public class TransactionQuery : FilterQuery
{
    public Guid? ConnectionId { get; set; }
    public bool? IncludeMonitor { get; set; }
    public bool? IncludePrivate { get; set; }
}

public enum TransactionsSortBy
{
    Start = 0,
    CountryCode = 1,
    Asn = 2,
    Client = 3,
    From = 4,
}

public enum LogsSortBy
{
    Timestamp = 0,
    Message = 1,
}

public enum RecipientsSortBy
{
    Username = 0,
    Data = 1,
}