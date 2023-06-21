using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Services;

namespace poshtar.Controllers;

/*
    var now = DateTime.UtcNow;
    xReq.Save($"{now:s}req.txt");
    xRes.Save($"{now:s}res.txt");
*/

[ApiController]
[AllowAnonymous]
[Route(DAV_PREFIX)]
public class DavController : ControllerBase
{
    static readonly SaveOptions xmlSaveOpt = SaveOptions.None; // Change in prod to SaveOptions.DisableFormatting
    const string DT_FORMAT = "o";
    const string DAV_PREFIX = "/dav/";
    const string WELL_KNOWN_PREFIX = "/.well-known/caldav/";
    const string PRINCIPAL_URI = DAV_PREFIX + "principal/";
    const string CALENDARS_URI = DAV_PREFIX + "cal/";
    readonly ILogger<DavController> _logger;
    readonly AppDbContext _db;

    public DavController(ILogger<DavController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [AcceptVerbs("PROPFIND")]
    [Route(WELL_KNOWN_PREFIX)]
    public async Task<IActionResult> WellKnown()
    {
        // Default URL for service discovery.

        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("WELLKNOWN 401");
            return Unauthenticated();
        }

        _logger.LogDebug("WELLKNOWN 207");
        var xReq = await GetRequestXmlAsync();
        var hrefName = X.nsDav.GetName("href");
        var props = xReq!.Descendants(X.nsDav.GetName("prop")).FirstOrDefault()!.Elements();
        var allprop = props.Elements(X.nsDav.GetName("allprops")).Any();

        var resourceTypeName = X.nsDav.GetName("resourcetype");
        var resourceType = !allprop && !props.Any(x => x.Name == resourceTypeName) ? null : resourceTypeName.Element(X.nsDav.Element("collection"));

        var currentUserPrincipalName = X.nsDav.GetName("current-user-principal");
        var currentUserPrincipal = !props.Any(x => x.Name == currentUserPrincipalName) ? null : currentUserPrincipalName.Element(hrefName.Element(PRINCIPAL_URI));

        var calendarHomeSetName = X.nsCalDav.GetName("calendar-home-set");
        var calendarHomeSet = !props.Any(x => x.Name == calendarHomeSetName) ? null : calendarHomeSetName.Element(hrefName.Element(CALENDARS_URI));

        var supportedProperties = new HashSet<XName> { resourceTypeName, currentUserPrincipalName, calendarHomeSetName };
        var prop404 = X.nsDav.Element("prop", props
                           .Where(p => !supportedProperties.Contains(p.Name))
                           .Select(p => new XElement(p.Name))
                   );

        var propStat404 = X.nsDav.Element("propstat", X.nsDav.Element("status", "HTTP/1.1 404 Not Found"), prop404);

        var xRes = X.nsDav.Element("multistatus",
            X.nsDav.Element("response",
                X.nsDav.Element("href", Request.Path),
                X.nsDav.Element("propstat",
                    X.nsDav.Element("prop", resourceType!, currentUserPrincipal!, calendarHomeSet!),
                    X.nsDav.Element("status", "HTTP/1.1 200 OK"),
                prop404.Elements().Any() ? propStat404 : null!
                )
            )
        );

        Response.Headers.Add("DAV", string.Join(", ", "1", "2", "3", "calendar-access"));
        Response.ContentType = "text/xml";
        Response.StatusCode = StatusCodes.Status207MultiStatus;
        using var stream = Response.BodyWriter.AsStream();

        await xRes.SaveAsync(stream, xmlSaveOpt, CancellationToken.None);

        return Empty;
    }

    [AcceptVerbs("PROPFIND")]
    [Route(PRINCIPAL_URI)]
    public async Task<IActionResult> Principal()
    {
        // Used to discover other URIs

        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("PRINCIPAL 401");
            return Unauthenticated();
        }

        _logger.LogDebug("PRINCIPAL 207");
        var xReq = await GetRequestXmlAsync();
        var hrefName = X.nsDav.GetName("href");
        var props = xReq!.Descendants(X.nsDav.GetName("prop")).FirstOrDefault()!.Elements();
        var allprop = props.Elements(X.nsDav.GetName("allprops")).Any();

        var ownerName = X.nsDav.GetName("owner");
        var owner = !allprop && !props.Any(x => x.Name == ownerName) ? null : ownerName.Element(X.nsDav.Element("href", PRINCIPAL_URI));

        var principalName = X.nsDav.GetName("current-user-principal");
        var principal = !allprop && !props.Any(x => x.Name == principalName) ? null : principalName.Element(X.nsDav.Element("href", PRINCIPAL_URI));

        var resourceTypeName = X.nsDav.GetName("resourcetype");
        var resourceType = !allprop && !props.Any(x => x.Name == resourceTypeName) ? null : resourceTypeName.Element(X.nsDav.Element("collection"), X.nsDav.Element("principal"));

        var calendarHomeSetName = X.nsCalDav.GetName("calendar-home-set");
        var calendarHomeSet = !props.Any(x => x.Name == calendarHomeSetName) ? null : calendarHomeSetName.Element(hrefName.Element(CALENDARS_URI));

        var supportedProperties = new HashSet<XName> { ownerName, principalName, resourceTypeName, calendarHomeSetName };
        var prop404 = X.nsDav.Element("prop", props
                           .Where(p => !supportedProperties.Contains(p.Name))
                           .Select(p => new XElement(p.Name))
                   );

        var propStat404 = X.nsDav.Element("propstat", X.nsDav.Element("status", "HTTP/1.1 404 Not Found"), prop404);

        var xRes = X.nsDav.Element("multistatus",
            X.nsDav.Element("response",
                X.nsDav.Element("href", Request.Path),
                X.nsDav.Element("propstat",
                    X.nsDav.Element("prop", owner!, principal!, resourceType!, calendarHomeSet!),
                    X.nsDav.Element("status", "HTTP/1.1 200 OK"),
                prop404.Elements().Any() ? propStat404 : null!
                )
            )
        );

        Response.Headers.Add("DAV", string.Join(", ", "1", "2", "3", "calendar-access"));
        Response.ContentType = "text/xml";
        Response.StatusCode = StatusCodes.Status207MultiStatus;
        using var stream = Response.BodyWriter.AsStream();

        await xRes.SaveAsync(stream, xmlSaveOpt, CancellationToken.None);

        return Empty;
    }

    [AcceptVerbs("PROPFIND")]
    [Route(CALENDARS_URI)]
    public async Task<IActionResult> Calendars()
    {
        // Discover calendar URIs for user

        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("CALENDARS 401");
            return Unauthenticated();
        }

        _logger.LogDebug("CALENDARS 207");
        var depth = 0; // 1 means include calendars
        if (Request.Headers.TryGetValue("Depth", out var depthStr) && int.TryParse(depthStr, out var depthVal))
            depth = depthVal;

        var xReq = await GetRequestXmlAsync();
        var hrefName = X.nsDav.GetName("href");
        var props = xReq!.Descendants(X.nsDav.GetName("prop")).FirstOrDefault()!.Elements();
        var allprop = props.Elements(X.nsDav.GetName("allprops")).Any();

        var resourceTypeName = X.nsDav.GetName("resourcetype");
        var resourceType = !allprop && !props.Any(x => x.Name == resourceTypeName) ? null : resourceTypeName.Element(X.nsDav.Element("collection"));
        var calendarResourceType = !allprop && !props.Any(x => x.Name == resourceTypeName) ? null : resourceTypeName.Element(X.nsDav.Element("collection"), X.nsCalDav.Element("calendar"));

        var displayNameName = X.nsDav.GetName("displayname");

        var supportedProperties = new HashSet<XName> { resourceTypeName, displayNameName };
        var prop404 = X.nsDav.Element("prop", props
                           .Where(p => !supportedProperties.Contains(p.Name))
                           .Select(p => new XElement(p.Name))
                   );

        var propStat404 = X.nsDav.Element("propstat", X.nsDav.Element("status", "HTTP/1.1 404 Not Found"), prop404);

        var userCalendars = await _db.Calendars
            .AsNoTracking()
            .Where(c => c.CalendarUsers.Any(cu => cu.UserId == userId.Value))
            .ToListAsync();

        var xRes = X.nsDav.Element("multistatus",
            X.nsDav.Element("response",
                X.nsDav.Element("href", Request.Path),
                X.nsDav.Element("propstat",
                    X.nsDav.Element("prop", resourceType!),
                    X.nsDav.Element("status", "HTTP/1.1 200 OK"),
                prop404.Elements().Any() ? propStat404 : null!
                )
            ),
            userCalendars.Select(uc =>
                X.nsDav.Element("response",
                    X.nsDav.Element("href", GetCalendarUri(uc.CalendarId)),
                    X.nsDav.Element("propstat",
                        X.nsDav.Element("prop",
                            calendarResourceType!,
                            displayNameName.Element(uc.DisplayName)
                        ),
                        X.nsDav.Element("status", "HTTP/1.1 200 OK")
                    )
                )
            )
        );

        Response.Headers.Add("DAV", string.Join(", ", "1", "2", "3", "calendar-access"));
        Response.ContentType = "text/xml";
        Response.StatusCode = StatusCodes.Status207MultiStatus;
        using var stream = Response.BodyWriter.AsStream();

        await xRes.SaveAsync(stream, xmlSaveOpt, CancellationToken.None);

        return Empty;
    }

    [AcceptVerbs("REPORT")]
    [Route(CALENDARS_URI + "{calendarId:int}")]
    public async Task<IActionResult> Report(int calendarId)
    {
        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("REPORT 401");
            return Unauthenticated();
        }

        _logger.LogDebug("REPORT 207");
        var xReq = await GetRequestXmlAsync();
        if (xReq == null)
            return BadRequest();

        var filterElm = xReq.Root!.Descendants(X.nsCalDav.GetName("filter")).FirstOrDefault();
        var hrefName = X.nsDav.GetName("href");
        var hrefs = xReq.Descendants(hrefName).Select(x => x.Value.Replace(GetCalendarUri(calendarId), string.Empty)).ToHashSet();
        var getetagName = X.nsDav.GetName("getetag");
        var getetag = xReq.Descendants(getetagName).FirstOrDefault();
        var calendarDataName = X.nsCalDav.GetName("calendar-data");
        var calendarData = xReq.Descendants(calendarDataName).FirstOrDefault();
        var getContentTypeName = X.nsDav.GetName("getcontenttype");
        var getContentType = xReq.Descendants(calendarDataName).FirstOrDefault();

        DateTime? lastModified = null;
        var query = _db.Calendars.Where(c => c.CalendarId == calendarId && c.CalendarUsers.Any(cu => cu.UserId == userId.Value));

        var operation = xReq.Root!.Name;
        if (operation == X.nsCalDav.GetName("calendar-multiget"))
            query = query.Include(c => c.CalendarItems.Where(e => hrefs.Contains(e.FileName)));
        else if (operation == X.nsDav.GetName("sync-collection"))
        {
            lastModified = await _db.Calendars
                .Where(c => c.CalendarId == calendarId && c.CalendarUsers.Any(cu => cu.UserId == userId.Value))
                .Select(c => c.CalendarItems.Max(e => e.Modified))
                .SingleAsync();

            var token = xReq.Root.Descendants(X.nsDav.GetName("sync-token")).FirstOrDefault()?.Value;
            if (!string.IsNullOrWhiteSpace(token) && DateTime.TryParse(token, out var since))
                query = query.Include(c => c.CalendarItems.Where(e => e.Modified > since.ToUniversalTime()));
            else
                query = query.Include(c => c.CalendarItems);
        }
        var userCalendar = await query.SingleAsync();
        var xRes = X.nsDav.Element("multistatus",
            userCalendar.CalendarItems.Select(r =>
                X.nsDav.Element("response",
                    X.nsDav.Element("href", GetCalendarItemUri(calendarId, r.FileName)),
                    X.nsDav.Element("propstat",
                        X.nsDav.Element("prop",
                            getetag == null ? null! : getetagName.Element(r.Modified.ToString(DT_FORMAT)),
                            getContentType == null ? null! : getContentTypeName.Element("text/calendar; component=vevent"), //vtodo
                            calendarData == null ? null! : calendarDataName.Element(GetCalendarItemDataFromFile(r.FileName))
                        ),
                        X.nsDav.Element("status", "HTTP/1.1 200 OK")
                    )
                )
            ),
            lastModified.HasValue ? X.nsDav.Element("sync-token", lastModified.Value.ToString(DT_FORMAT)) : null!
        );

        Response.ContentType = "text/xml";
        Response.StatusCode = StatusCodes.Status207MultiStatus;
        using var stream = Response.BodyWriter.AsStream();

        await xRes.SaveAsync(stream, xmlSaveOpt, CancellationToken.None);

        return Empty;
    }

    [HttpHead]
    public async Task<IActionResult> Head()
    {
        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("HEAD 401");
            return Unauthenticated();
        }

        _logger.LogDebug("HEAD 200");
        return Ok();
    }
    [HttpOptions]
    [Route(PRINCIPAL_URI)]
    [Route(CALENDARS_URI)]
    [Route(CALENDARS_URI + "{calendarId:int?}")]
    public async Task<IActionResult> Options(int? calendarId)
    {
        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("OPTIONS 401");
            return Unauthenticated();
        }

        _logger.LogDebug("OPTIONS 200");
        Response.Headers.Add("DAV", string.Join(", ", "1", "2", "3", "calendar-access"));
        Response.Headers.Allow = "OPTIONS, HEAD, DELETE, PROPFIND, PUT, REPORT";
        return Ok();
    }

    [AcceptVerbs("PROPFIND")]
    [Route(CALENDARS_URI + "{calendarId:int}")]
    public async Task<IActionResult> Propfind(int calendarId)
    {
        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("PROPFIND 401");
            return Unauthenticated();
        }

        _logger.LogDebug("PROPFIND 207");
        var depth = 0; // 1 means include calendar items
        if (Request.Headers.TryGetValue("Depth", out var depthStr) && int.TryParse(depthStr, out var depthVal))
            depth = depthVal;

        var query = _db.Calendars
            .AsNoTracking()
            .Where(c => c.CalendarId == calendarId && c.CalendarUsers.Any(cu => cu.UserId == userId.Value));

        if (depth > 0)
            query = query.Include(c => c.CalendarItems);

        var calendar = await query.SingleAsync();

        var xReq = await GetRequestXmlAsync();
        var props = xReq!.Descendants(X.nsDav.GetName("prop")).FirstOrDefault()!.Elements();
        var allprop = props.Elements(X.nsDav.GetName("allprops")).Any();

        var supportedReportSetName = X.nsDav.GetName("supported-report-set");
        var supportedReportSet = !allprop && !props.Any(x => x.Name == supportedReportSetName) ? null :
            supportedReportSetName.Element(
                X.nsDav.Element("supported-report", X.nsDav.Element("report", X.nsDav.Element("sync-collection"))),
                X.nsDav.Element("supported-report", X.nsDav.Element("report", X.nsCalDav.Element("calendar-multiget")))
            // X.nsDav.Element("supported-report", X.nsDav.Element("report", X.nsCalDav.Element("calendar-query"))),
            // X.nsDav.Element("supported-report", X.nsDav.Element("report", X.nsCalDav.Element("free-busy-query")))
            );

        var ownerName = X.nsDav.GetName("owner");
        var owner = !allprop && !props.Any(x => x.Name == ownerName) ? null : ownerName.Element(X.nsDav.Element("href", PRINCIPAL_URI));

        var principalName = X.nsDav.GetName("current-user-principal");
        var principal = !allprop && !props.Any(x => x.Name == principalName) ? null : principalName.Element(X.nsDav.Element("href", PRINCIPAL_URI));

        var getctagName = X.nsCalSrv.GetName("getctag");
        var syncTokenName = X.nsDav.GetName("sync-token");
        XElement getctag = null!;
        XElement syncToken = null!;
        if (allprop || (props.Any(x => x.Name == getctagName) || props.Any(x => x.Name == syncTokenName)))
        {
            var lastModified = await _db.CalendarItems.Where(ci => ci.CalendarId == calendarId).MaxAsync(ci => ci.Modified);
            getctag = getctagName.Element(lastModified.ToString(DT_FORMAT));
            syncToken = syncTokenName.Element(lastModified.ToString(DT_FORMAT));
        }

        var getetagName = X.nsDav.GetName("getetag");
        var getetag = getetagName.Element();

        var resourceTypeName = X.nsDav.GetName("resourcetype");
        var resourceType = !allprop && !props.Any(x => x.Name == resourceTypeName) ? null : resourceTypeName.Element(X.nsDav.Element("collection"), X.nsCalDav.Element("calendar"));

        var supportedComponentsName = X.nsCalDav.GetName("supported-calendar-component-set");
        var supportedComponents = !allprop && !props.Any(x => x.Name == supportedComponentsName) ? null :
            supportedComponentsName.Element(
                X.nsCalDav.Element("comp", new XAttribute("name", "VEVENT")),
                X.nsCalDav.Element("comp", new XAttribute("name", "VTODO")),
                X.nsCalDav.Element("comp", new XAttribute("name", "VJURNAL"))
            );

        var getContentTypeName = X.nsDav.GetName("getcontenttype");
        var getContentType = !allprop && !props.Any(x => x.Name == getContentTypeName) ? null : getContentTypeName.Element("text/calendar; component=vevent");

        var displayNameName = X.nsDav.GetName("displayname");
        var displayName = !allprop && !props.Any(x => x.Name == displayNameName) ? null : displayNameName.Element(calendar.DisplayName);

        var supportedProperties = new HashSet<XName> { ownerName, principalName, resourceTypeName, supportedComponentsName, supportedReportSetName, getContentTypeName, displayNameName, getctagName, syncTokenName, getetagName };
        var prop404 = X.nsDav.Element("prop", props
                    .Where(p => !supportedProperties.Contains(p.Name))
                    .Select(p => new XElement(p.Name))
            );

        var propStat404 = X.nsDav.Element("propstat", X.nsDav.Element("status", "HTTP/1.1 404 Not Found"), prop404);

        var xRes = X.nsDav.Element("multistatus",
            X.nsDav.Element("response",
                X.nsDav.Element("href", Request.Path),
                X.nsDav.Element("propstat",
                    X.nsDav.Element("prop",
                        resourceType!,
                        owner!,
                        principal!,
                        displayName!,
                        getContentType!,
                        supportedReportSet!,
                        supportedComponents!,
                        getctag!,
                        syncToken!
                    ),
                    X.nsDav.Element("status", "HTTP/1.1 200 OK")
                ),
                prop404.Elements().Any() ? propStat404 : null!
            ),
            calendar.CalendarItems.Select(e =>
                X.nsDav.Element("response",
                    X.nsDav.Element("href", GetCalendarItemUri(e.CalendarId, e.FileName)),
                    X.nsDav.Element("propstat",
                        resourceType == null ? null! : resourceTypeName.Element(),
                        getContentType == null ? null! : getContentTypeName.Element("text/calendar; charset=utf-8; component=vevent"),
                        getetag == null ? null! : getetagName.Element(e.Modified.ToString(DT_FORMAT)),
                        X.nsDav.Element("status", "HTTP/1.1 200 OK")
                    )
                )
            )
        );

        Response.Headers.Add("DAV", string.Join(", ", "1", "2", "3", "calendar-access"));
        Response.ContentType = "text/xml";
        Response.StatusCode = StatusCodes.Status207MultiStatus;
        using var stream = Response.BodyWriter.AsStream();

        await xRes.SaveAsync(stream, xmlSaveOpt, CancellationToken.None);

        return Empty;
    }
    [AcceptVerbs("GET")]
    [Route(CALENDARS_URI + "{calendarId:int}/{itemFileName}")]
    public async Task<IActionResult> Get(int calendarId, string itemFileName)
    {
        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("GET 401");
            return Unauthenticated();
        }

        _logger.LogDebug("GET 200");
        var calendar = await _db.Calendars
            .Where(c => c.CalendarId == calendarId && c.CalendarUsers.Any(cu => cu.UserId == userId.Value))
            .Include(c => c.CalendarItems.Where(ce => ce.FileName == itemFileName))
            .SingleAsync();

        var calendarItem = calendar.CalendarItems.FirstOrDefault();
        if (calendarItem == null)
            return NotFound();

        var path = C.Paths.CalendarItemsDataFor(calendarItem.FileName);
        if (!System.IO.File.Exists(path))
            return NotFound();

        Response.Headers.Add("DAV", string.Join(", ", "1", "2", "3", "calendar-access"));
        Response.ContentType = "text/calendar";
        Response.StatusCode = StatusCodes.Status200OK;
        using var fs = System.IO.File.OpenRead(path);
        using var stream = Response.BodyWriter.AsStream();

        await fs.CopyToAsync(stream);
        return Empty;
    }
    [AcceptVerbs("PUT")]
    [Route(CALENDARS_URI + "{calendarId:int}/{itemFileName}")]
    public async Task<IActionResult> Put(int calendarId, string itemFileName)
    {
        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("PUT 401");
            return Unauthenticated();
        }

        _logger.LogDebug("PUT 200");
        var data = await GetCalendarItemDataFromRequestAsync();
        if (string.IsNullOrWhiteSpace(data))
            return BadRequest();

        var calendar = await _db.Calendars
            .Where(c => c.CalendarId == calendarId && c.CalendarUsers.Any(cu => cu.UserId == userId.Value))
            .Include(c => c.CalendarItems.Where(ce => ce.FileName == itemFileName))
            .SingleAsync();

        var existing = calendar.CalendarItems.FirstOrDefault();
        var now = DateTime.UtcNow;
        var isNew = existing == null;
        if (isNew)
            calendar.CalendarItems.Add(new()
            {
                Type = CalendarItemType.Unknown, // TODO
                FileName = itemFileName,
                Modified = now,
            });
        else
            existing!.Modified = now;

        await System.IO.File.WriteAllTextAsync(C.Paths.CalendarItemsDataFor(itemFileName), data);
        await _db.SaveChangesAsync();

        Response.Headers.ETag = now.ToString(DT_FORMAT);
        return isNew ? StatusCode(StatusCodes.Status201Created) : StatusCode(StatusCodes.Status204NoContent);
    }
    [AcceptVerbs("DELETE")]
    [Route(CALENDARS_URI + "{calendarId:int}/{itemFileName}")]
    public async Task<IActionResult> Delete(int calendarId, string itemFileName)
    {
        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("DELETE 401");
            return Unauthenticated();
        }

        _logger.LogDebug("DELETE 200");
        var calendar = await _db.Calendars
            .Where(c => c.CalendarId == calendarId && c.CalendarUsers.Any(cu => cu.UserId == userId.Value))
            .Include(c => c.CalendarItems.Where(ce => ce.FileName == itemFileName))
            .SingleAsync();

        var existing = calendar.CalendarItems.FirstOrDefault();
        if (existing == null)
            return StatusCode(StatusCodes.Status404NotFound);

        _db.CalendarItems.Remove(existing);
        System.IO.File.Delete(C.Paths.CalendarItemsDataFor(itemFileName));
        await _db.SaveChangesAsync();
        return StatusCode(StatusCodes.Status204NoContent);
    }


    // Helpers
    string GetCalendarUri(int calendarId) => $"{CALENDARS_URI}{calendarId}/";
    string GetCalendarItemUri(int calendarId, string itemFileName) => $"{CALENDARS_URI}{calendarId}/{itemFileName}";
    async Task<int?> Authenticate()
    {
        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var authHeader) || !authHeader.Scheme.Equals("Basic") || string.IsNullOrWhiteSpace(authHeader.Parameter))
            return null;

        string userName = string.Empty, password = string.Empty;
        try
        {
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
            userName = credentials[0].ToLower();
            password = credentials[1];
        }
        catch (Exception) { }

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            return null;

        var notOutUserName = userName;
        var user = await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Name == notOutUserName && !u.Disabled.HasValue);

        if (user == null || !DovecotHasher.Verify(user.Salt, user.Hash, password))
            return null;

        return user.UserId;
    }
    IActionResult Unauthenticated()
    {
        //Response.Headers.ContentType = string.Join("; ", "application/xml", "charset=utf-8");
        Response.Headers.WWWAuthenticate = string.Join(", ", "Basic realm=\"poshtar\"", "charset=\"UTF-8\"");
        return StatusCode(StatusCodes.Status401Unauthorized);
    }
    async Task<XDocument?> GetRequestXmlAsync()
    {
        if (!(Request.ContentType ?? string.Empty).ToLower().Contains("xml") || Request.ContentLength == 0)
            return null;

        using var stream = Request.BodyReader.AsStream();
        return await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
    }
    async Task<string?> GetCalendarItemDataFromRequestAsync()
    {
        if (!(Request.ContentType ?? string.Empty).ToLower().Contains("calendar") || Request.ContentLength == 0)
            return null;

        using var stream = Request.BodyReader.AsStream();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
    string GetCalendarItemDataFromFile(string fileName)
    {
        var path = C.Paths.CalendarItemsDataFor(fileName);
        var contents = System.IO.File.ReadAllText(path);
        return contents;
    }
}