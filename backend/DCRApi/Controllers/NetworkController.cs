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
        Console.WriteLine($"Received connect request: {req.Node.URL}");
        _logger.LogTrace($"Received connect request: {req}");
        var clientNeighbors = DeepCopyNodes(_networkClient.ClientNeighbors);
        _networkClient.AddNode(req.Node);
        foreach (var neighbor in req.Neighbors) {
            _networkClient.ConnectToPeerNetwork(neighbor);
        }

        PrintNeighborList();

        return Ok(clientNeighbors);
    }

    [HttpPost("disconnect")]
    public IActionResult Disconnect([FromBody] ConnectNode req)
    {
        Console.WriteLine($"Received disconnect request: {req.Node.URL}");
        _logger.LogTrace($"Received disconnect request: {req}");
        _networkClient.RemoveNode(req.Node);

        PrintNeighborList();

        return Ok();
    }

    private List<Node> DeepCopyNodes(List<Node> nodes)
    {
        return _networkSerializer.Deserialize(_networkSerializer.Serialize(nodes));
    }

    private void PrintNeighborList() {
        Console.WriteLine($"Updated Neighbor List:");
        foreach (var neighbor in _networkClient.ClientNeighbors) {
            Console.WriteLine(neighbor.URL);
        }
    }
}