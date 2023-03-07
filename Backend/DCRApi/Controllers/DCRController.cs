using Microsoft.AspNetCore.Mvc;
namespace DCR;
[ApiController]
[Route("[controller]")]
public class DCRController : ControllerBase
{

    private readonly ILogger<DCRController> _logger;

    public DCRController(ILogger<DCRController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{id}")]
    public string Get(string id)
    {
        _logger.LogTrace("Tried getting the DCR id");
        return $"Tried getting id : {id}";
    }

    [HttpPost("{id}")]
    public string Post(string id)
    {
        _logger.LogTrace("Tried posting the DCR id");
        return $"Tried posting id : {id}";
    }

    [HttpPost("updategraph/{id}")]
    public string Update(string id)
    {
        _logger.LogTrace("Tried updating the DCR id");
        return $"Tried updating id : {id}";
    }
}