using Microsoft.AspNetCore.Mvc;

namespace DNS;
[ApiController]
[Route("[controller]")]
public class DNSController : ControllerBase
{

    private readonly ILogger<DNSController> _logger;

    public DNSController(ILogger<DNSController> logger)
    {
        _logger = logger;
    }

    [HttpGet()]
    public IActionResult Get()
    {
        _logger.LogTrace($"Sending neighbors");
        _logger.LogInformation($"Sending neighbors");
        var network = System.IO.File.ReadAllText("network.json");
        return Ok(network);
    }
}