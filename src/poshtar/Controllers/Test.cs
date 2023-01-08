using Microsoft.AspNetCore.Mvc;

namespace testis.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetTest")]
    public IEnumerable<int> Get()
    {
        return Enumerable.Range(1, 5).ToArray();
    }
}