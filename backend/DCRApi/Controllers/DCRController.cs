using Microsoft.AspNetCore.Mvc;
using Models;

namespace DCR;
[ApiController]
[Route("[controller]")]
public class DCRController : ControllerBase
{

    private readonly ILogger<DCRController> _logger;
    private readonly FullNode _node;
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    private readonly GraphSerializer _graphSerializer = new GraphSerializer();

    public DCRController(ILogger<DCRController> logger, FullNode node)
    {
        _node = node;
        _logger = logger;
    }

    [HttpPost("create")]
    public IActionResult CreateGraph([FromBody] CreateGraphRequest req)
    {
        _logger.LogInformation($"Adding graph to blockchain : {req}");
        var graph = new Graph(req.Activities, req.Relations);
        var tx = new Transaction(req.Actor, Action.Create, "", graph);
        _node.HandleTransaction(tx);
        _node.AddDiscoveredGraph(graph);
        _logger.LogInformation($"Block validity: {_node.Blockchain.IsValid()}");
        _logger.LogInformation($"Created Transaction");
        return Ok(graph);
    }

    [HttpGet("graph/{id}")]
    public IActionResult GetGraph(string id)
    {
        _logger.LogTrace($"Fetching graph {id}");
        _logger.LogInformation($"Block validity: {_node.Blockchain.IsValid()}");
        // TODO Modify getgraph so it only returns if graph is 8 blocks deep
        var graph = _node.Blockchain.GetGraph(id)!;
        if (graph is not null)
        {
            _node.AddDiscoveredGraph(graph);
            return Ok(graph);
        }
        return NotFound("Could not find graph");
    }

    [HttpPut("execute/{id}")]
    public IActionResult ExecuteActivity(string id, ExecuteActivityRequest req)
    {
        _logger.LogInformation($"Executing activity {req.ExecutingActivity} in graph {id}");
        var graph = _node.Blockchain.GetGraph(id)!;
        if (graph is null)
        {
            return NotFound("Could not find graph");
        }
        var tx = CreateUpdateGraphTransaction(graph, req.Actor, req.ExecutingActivity);
        _node.HandleTransaction(tx);
        _logger.LogInformation($"Block validity: {_node.Blockchain.IsValid()}");
        _logger.LogInformation($"Updated graph {graph.Id}");
        return Ok("Transaction added");
    }

    [HttpGet("discovered")]
    public IActionResult GetDiscoveredGraphs()
    {
        return Ok(_node.DiscoveredGraphs);
    }

    [HttpGet("pending")]
    public IActionResult GetPendingTransactions()
    {
        return Ok(_node.PendingTransactions);
    }

    private Transaction CreateUpdateGraphTransaction(Graph graph, string actor, string executingActivity)
    {
        graph.Execute(executingActivity);
        return new Transaction(actor, Action.Update, executingActivity, graph);
    }
}