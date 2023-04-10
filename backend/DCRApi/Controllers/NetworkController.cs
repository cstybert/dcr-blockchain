using Microsoft.AspNetCore.Mvc;


namespace DCR;
[ApiController]
[Route("[controller]")]
public class NetworkController : ControllerBase
{

    private readonly ILogger<NetworkController> _logger;
    private readonly NetworkClient _networkClient;
    private readonly NetworkSerializer _networkSerializer;

    public NetworkController(ILogger<NetworkController> logger, NetworkClient networkClient)
    {
        _logger = logger;
        _networkClient = networkClient;
        _networkSerializer = new NetworkSerializer();
    }

    [HttpPost("connect")]
    public IActionResult Connect([FromBody] ConnectNode req)
    {
        _logger.LogTrace($"Received connect request: {req}");
        var clientNeighbors = DeepCopyNodes(_networkClient.ClientNeighbors);
        _networkClient.ConnectToPeerNetwork(req.Node);

        return Ok(clientNeighbors);
    }

    [HttpPost("disconnect")]
    public IActionResult Disconnect([FromBody] ConnectNode req)
    {
        _logger.LogTrace($"Received disconnect request: {req}");
        _networkClient.RemoveNode(req.Node);
        Console.WriteLine($"Disconnected from node {req.Node.URL}");

        return Ok();
    }

    private List<NetworkNode> DeepCopyNodes(List<NetworkNode> nodes)
    {
        return _networkSerializer.Deserialize(_networkSerializer.Serialize(nodes));
    }
}