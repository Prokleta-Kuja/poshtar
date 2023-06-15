using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using poshtar.Entities;

namespace poshtar.Controllers;

[ApiController]
[Route(URI_PREFIX)]
public class DavController : ControllerBase
{
    static readonly SaveOptions xmlSaveOpt = SaveOptions.None; // Change in prod to SaveOptions.DisableFormatting
    const string DT_FORMAT = "s";
    const string URI_PREFIX = "/.well-known/caldav/";
    readonly ILogger<DavController> _logger;
    readonly AppDbContext _db;

    public DavController(ILogger<DavController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }
}