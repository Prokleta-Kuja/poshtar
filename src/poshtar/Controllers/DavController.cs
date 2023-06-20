using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Services;

namespace poshtar.Controllers;

[ApiController]
[AllowAnonymous]
[Route(DAV_PREFIX)]
public class DavController : ControllerBase
{
    static readonly SaveOptions xmlSaveOpt = SaveOptions.None; // Change in prod to SaveOptions.DisableFormatting
    const string DT_FORMAT = "s";
    const string DAV_PREFIX = "/dav/";
    const string WELL_KNOWN_PREFIX = "/.well-known/caldav/";
    const string PRINCIPAL_URI = WELL_KNOWN_PREFIX + "principal";
    const string CALENDARS_URI = WELL_KNOWN_PREFIX + "calendars";
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
        // This is the default URL for service discovery.

        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("WELLKNOWN 401");
            return Unauthenticated();
        }

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
        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("PRINCIPAL 401");
            return Unauthenticated();
        }

        var xReq = await GetRequestXmlAsync();
        var hrefName = X.nsDav.GetName("href");
        var props = xReq!.Descendants(X.nsDav.GetName("prop")).FirstOrDefault()!.Elements();
        var allprop = props.Elements(X.nsDav.GetName("allprops")).Any();

        var resourceTypeName = X.nsDav.GetName("resourcetype");
        var resourceType = !allprop && !props.Any(x => x.Name == resourceTypeName) ? null : resourceTypeName.Element(X.nsDav.Element("collection"), X.nsDav.Element("principal"));

        var calendarHomeSetName = X.nsCalDav.GetName("calendar-home-set");
        var calendarHomeSet = !props.Any(x => x.Name == calendarHomeSetName) ? null : calendarHomeSetName.Element(hrefName.Element(CALENDARS_URI));

        var supportedProperties = new HashSet<XName> { resourceTypeName, calendarHomeSetName };
        var prop404 = X.nsDav.Element("prop", props
                           .Where(p => !supportedProperties.Contains(p.Name))
                           .Select(p => new XElement(p.Name))
                   );

        var propStat404 = X.nsDav.Element("propstat", X.nsDav.Element("status", "HTTP/1.1 404 Not Found"), prop404);

        var xRes = X.nsDav.Element("multistatus",
            X.nsDav.Element("response",
                X.nsDav.Element("href", Request.Path),
                X.nsDav.Element("propstat",
                    X.nsDav.Element("prop", resourceType!, calendarHomeSet!),
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
        var now = DateTime.UtcNow;
        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("CALENDARS 401");
            return Unauthenticated();
        }

        var depth = 0; // 1 means include calendars
        if (Request.Headers.TryGetValue("Depth", out var depthStr) && int.TryParse(depthStr, out var depthVal))
            depth = depthVal;

        var xReq = await GetRequestXmlAsync();
        xReq.Save($"{now:s}req.txt");
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

        xRes.Save($"{now:s}res.txt");
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
    public async Task<IActionResult> Options()
    {
        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("OPTIONS 401");
            return Unauthenticated();
        }

        _logger.LogDebug("OPTIONS 200");

        Response.Headers.Add("DAV", string.Join(", ", "1", "2", "3", "calendar-access"));
        var xdoc = await GetRequestXmlAsync();
        if (xdoc != null)
        {
            var name = xdoc.Root?.Elements().FirstOrDefault()?.Name.LocalName.ToLower();
            if (!string.IsNullOrWhiteSpace(name) && name.Equals("calendar-collection-set"))
            {
                var userCalendars = await _db.Calendars
                    .AsNoTracking()
                    .Where(c => c.CalendarUsers.Any(cu => cu.UserId == userId.Value))
                    .ToListAsync();

                var xRes = X.nsDav.Element("options-response",
                    X.nsCalDav.Element("calendar-collection-set",
                        userCalendars.Select(uc =>
                            X.nsDav.Element("href", GetCalendarUri(uc.CalendarId)))
                    )
                );
                Response.ContentType = "text/xml";
                Response.StatusCode = StatusCodes.Status200OK;
                using var stream = Response.BodyWriter.AsStream();

                await xRes.SaveAsync(stream, xmlSaveOpt, CancellationToken.None);

                return Empty;
            }
        }

        Response.Headers.Allow = "OPTIONS, HEAD, DELETE, PROPFIND, PUT, REPORT";
        return Ok();
    }

    [AcceptVerbs("PROPFIND")]
    [Route("{calendarId:int?}")]
    public async Task<IActionResult> Propfind(int calendarId)
    {
        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("PROPFIND 401");
            return Unauthenticated();
        }

        var depth = 0; // 1 means include events
        if (Request.Headers.TryGetValue("Depth", out var depthStr) && int.TryParse(depthStr, out var depthVal))
            depth = depthVal;

        var query = _db.Calendars
            .AsNoTracking()
            .Where(c => c.CalendarId == calendarId && c.CalendarUsers.Any(cu => cu.UserId == userId.Value));

        if (depth > 0)
            query = query.Include(c => c.CalendarEvents);

        var calendar = await query.SingleAsync();

        _logger.LogDebug("PROPFIND 200");
        var xReq = await GetRequestXmlAsync();
        var props = xReq!.Descendants(X.nsDav.GetName("prop")).FirstOrDefault()!.Elements();
        var allprop = props.Elements(X.nsDav.GetName("allprops")).Any();

        var supportedReportSetName = X.nsDav.GetName("supported-report-set");
        var supportedReportSet = !allprop && !props.Any(x => x.Name == supportedReportSetName) ? null :
            supportedReportSetName.Element(
                X.nsDav.Element("supported-report", X.nsDav.Element("report", X.nsDav.Element("sync-collection"))),
                X.nsDav.Element("supported-report", X.nsDav.Element("report", X.nsDav.Element("calendar-multiget")))
            // X.nsDav.Element("supported-report", X.nsDav.Element("report", X.nsDav.Element("calendar-query"))),
            // X.nsDav.Element("supported-report", X.nsDav.Element("report", X.nsDav.Element("free-busy-query")))
            );

        var getctagName = X.nsCalSrv.GetName("getctag");
        var getctag = getctagName.Element();

        // XElement getctag = null!;
        // if (!allprop && !props.Any(x => x.Name == getctagName))
        // {
        //     if (lastModified == DateTime.MinValue)
        //         lastModified = VEvents.Max(e => e.Modified);
        //     getctag = getctagName.Element(lastModified.ToString(DT_FORMAT));
        // }

        var getetagName = X.nsDav.GetName("getetag");
        var getetag = getetagName.Element();

        // XElement getetag = null!;
        // if (!allprop && !props.Any(x => x.Name == getetagName))
        // {
        //     if (lastModified == DateTime.MinValue)
        //         lastModified = VEvents.Max(e => e.Modified);
        //     getetag = getetagName.Element(lastModified.ToString(DT_FORMAT));
        // }

        var resourceTypeName = X.nsDav.GetName("resourcetype");
        var resourceType = !allprop && !props.Any(x => x.Name == resourceTypeName) ? null : resourceTypeName.Element(X.nsDav.Element("collection"), X.nsCalDav.Element("calendar"));

        var supportedComponentsName = X.nsCalDav.GetName("supported-calendar-component-set");
        var supportedComponents = !allprop && !props.Any(x => x.Name == supportedComponentsName) ? null : supportedComponentsName.Element(X.nsCalDav.Element("comp", new XAttribute("name", "VEVENT")));

        var getContentTypeName = X.nsDav.GetName("getcontenttype");
        var getContentType = !allprop && !props.Any(x => x.Name == getContentTypeName) ? null : getContentTypeName.Element("text/calendar; component=vevent");

        var displayNameName = X.nsDav.GetName("displayname");
        var displayName = !allprop && !props.Any(x => x.Name == displayNameName) ? null : displayNameName.Element(calendar.DisplayName);

        var supportedProperties = new HashSet<XName> { resourceTypeName, supportedComponentsName, getContentTypeName, displayNameName };
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
                        displayName!,
                        getContentType!,
                        supportedReportSet!,
                        supportedComponents!
                    ),
                    X.nsDav.Element("status", "HTTP/1.1 200 OK"),
                prop404.Elements().Any() ? propStat404 : null!
                )
            ),
            calendar.CalendarEvents.Select(e =>
                X.nsDav.Element("response",
                    X.nsDav.Element("href", GetCalendarEventUri(e.CalendarId, e.FileName)),
                    X.nsDav.Element("propstat",
                        resourceType == null ? null! : resourceTypeName.Element(),
                        getContentType == null ? null! : getContentTypeName.Element("text/calendar; component=vevent"),
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
    [Route("{calendarId:int}/{eventFileName}")]
    public async Task<IActionResult> Get(int calendarId, string eventFileName)
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
            .Include(c => c.CalendarEvents.Where(ce => ce.FileName == eventFileName))
            .SingleAsync();

        var calendarEvent = calendar.CalendarEvents.FirstOrDefault();
        if (calendarEvent == null)
            return NotFound();

        var path = C.Paths.EventDataFor(calendarEvent.FileName);
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
    [Route("{calendarId:int}/{eventFileName}")]
    public async Task<IActionResult> Put(int calendarId, string eventFileName)
    {
        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("PUT 401");
            return Unauthenticated();
        }

        _logger.LogDebug("PUT 200");
        var data = await GetCalendarEventDataFromRequestAsync();
        if (string.IsNullOrWhiteSpace(data))
            return BadRequest();

        var calendar = await _db.Calendars
            .Where(c => c.CalendarId == calendarId && c.CalendarUsers.Any(cu => cu.UserId == userId.Value))
            .Include(c => c.CalendarEvents.Where(ce => ce.FileName == eventFileName))
            .SingleAsync();

        var existing = calendar.CalendarEvents.FirstOrDefault();
        var now = DateTime.UtcNow;
        var isNew = existing == null;
        if (isNew)
            calendar.CalendarEvents.Add(new()
            {
                FileName = eventFileName,
                Modified = now,
            });
        else
            existing!.Modified = now;

        await System.IO.File.WriteAllTextAsync(C.Paths.EventDataFor(eventFileName), data);
        await _db.SaveChangesAsync();

        Response.Headers.ETag = now.ToString(DT_FORMAT);
        return isNew ? StatusCode(StatusCodes.Status201Created) : StatusCode(StatusCodes.Status204NoContent);
    }
    [AcceptVerbs("DELETE")]
    [Route("{calendarId:int}/{eventFileName}")]
    public async Task<IActionResult> Delete(int calendarId, string eventFileName)
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
            .Include(c => c.CalendarEvents.Where(ce => ce.FileName == eventFileName))
            .SingleAsync();

        var existing = calendar.CalendarEvents.FirstOrDefault();
        if (existing == null)
            return StatusCode(StatusCodes.Status404NotFound);

        _db.CalendarEvents.Remove(existing);
        System.IO.File.Delete(C.Paths.EventDataFor(eventFileName));
        await _db.SaveChangesAsync();
        return StatusCode(StatusCodes.Status204NoContent);
    }
    [AcceptVerbs("REPORT")]
    [Route("{calendarId:int}")]
    public async Task<IActionResult> Report(int calendarId)
    {
        var userId = await Authenticate();
        if (!userId.HasValue)
        {
            _logger.LogDebug("REPORT 401");
            return Unauthenticated();
        }

        _logger.LogDebug("REPORT 200");
        var xReq = await GetRequestXmlAsync();
        if (xReq == null)
            return BadRequest();

        var filterElm = xReq.Root!.Descendants(X.nsCalDav.GetName("filter")).FirstOrDefault();
        var hrefName = X.nsDav.GetName("href");
        var hrefs = xReq.Descendants(hrefName).Select(x => x.Value.Replace(DAV_PREFIX, string.Empty)).ToHashSet();
        var getetagName = X.nsDav.GetName("getetag");
        var getetag = xReq.Descendants(getetagName).FirstOrDefault();
        var calendarDataName = X.nsCalDav.GetName("calendar-data");
        var calendarData = xReq.Descendants(calendarDataName).FirstOrDefault();

        DateTime? lastModified = null;
        var query = _db.Calendars.Where(c => c.CalendarId == calendarId && c.CalendarUsers.Any(cu => cu.UserId == userId.Value));

        var operation = xReq.Root!.Name;
        if (operation == X.nsCalDav.GetName("calendar-multiget"))
            query = query.Include(c => c.CalendarEvents.Where(e => hrefs.Contains(e.FileName)));
        else if (operation == X.nsDav.GetName("sync-collection"))
        {
            var token = xReq.Root.Descendants(X.nsDav.GetName("sync-token")).FirstOrDefault()?.Value;
            if (!string.IsNullOrWhiteSpace(token) && DateTime.TryParse(token, out var since))
                query = query.Include(c => c.CalendarEvents.Where(e => e.Modified > since));
            else
                query = query.Include(c => c.CalendarEvents);

            lastModified = await _db.Calendars
                .Where(c => c.CalendarId == calendarId && c.CalendarUsers.Any(cu => cu.UserId == userId.Value))
                .Select(c => c.CalendarEvents.Max(e => e.Modified))
                .SingleAsync();
        }
        var userCalendar = await query.SingleAsync();
        var xRes = X.nsDav.Element("multistatus",
            userCalendar.CalendarEvents.Select(async r =>
                X.nsDav.Element("response",
                    X.nsDav.Element("href", GetCalendarEventUri(calendarId, r.FileName)),
                    X.nsDav.Element("propstat",
                        X.nsDav.Element("prop",
                            getetag == null ? null! : X.nsDav.Element("getetag", r.Modified.ToString(DT_FORMAT)),
                            calendarData == null ? null! : X.nsCalDav.Element("calendar-data", await GetCalendarEventDataFromFile(r.FileName))
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

    // Helpers
    string GetCalendarUri(int calendarId) => $"{DAV_PREFIX}{calendarId}/";
    string GetCalendarEventUri(int calendarId, string eventFileName) => $"{DAV_PREFIX}{calendarId}/{eventFileName}";
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
    async Task<string?> GetCalendarEventDataFromRequestAsync()
    {
        if (!(Request.ContentType ?? string.Empty).ToLower().Contains("calendar") || Request.ContentLength == 0)
            return null;

        using var stream = Request.BodyReader.AsStream();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
    async Task<string> GetCalendarEventDataFromFile(string fileName)
    {
        var path = C.Paths.EventDataFor(fileName);
        var contents = await System.IO.File.ReadAllTextAsync(path);
        return contents;
    }
}