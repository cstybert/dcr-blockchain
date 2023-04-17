using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Models;
using Business;

namespace DCR;
[ApiController]
[Route("[controller]")]
public class DCRController : ControllerBase
{

    private readonly ILogger<DCRController> _logger;
    private readonly FullNode _node;
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    private readonly GraphSerializer _graphSerializer = new GraphSerializer();
    private readonly GraphCreator _graphCreator = new GraphCreator();
    private readonly GraphExecutor _graphExecutor = new GraphExecutor();

    public DCRController(ILogger<DCRController> logger, FullNode node)
    {
        _node = node;
        _logger = logger;
    }

    [HttpGet("fetched")]
    public IActionResult GetFetchedGraphs()
    {
        return Ok(_node.FetchedGraphs);
    }

    [HttpGet("pending")]
    public IActionResult GetPendingTransactions()
    {
        return Ok(_node.PendingTransactions);
    }

    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        _logger.LogTrace($"Fetching graph {id}");
        _logger.LogInformation($"Block validity: {_node.Blockchain.IsValid()}");
        // TODO Modify getgraph so it only returns if graph is 8 blocks deep
        var graph = _node.Blockchain.GetGraph(id)!;
        if (graph is not null)
        {
            _node.AddFetchedGraph(graph);
            return Ok(_node.Blockchain.GetGraph(id));
        }
        return NotFound("Could not find graph");
    }

    [HttpPost("create")]
    public IActionResult Post([FromBody] CreateGraphRequest req)
    {
        _logger.LogInformation($"Adding graph to blockchain : {req}");
        var graph = _graphCreator.Create(req.Activities, req.Relations);
        var tx = new Transaction(req.Actor, Action.Create, "", graph);
        _node.HandleTransaction(tx);
        _logger.LogInformation($"Block validity: {_node.Blockchain.IsValid()}");
        _logger.LogInformation($"Created Transaction");
        _node.AddPendingTransaction(tx);
        return Ok(graph);
    }

    [HttpPut("update/{id}")]
    public IActionResult Put(string id, ExecuteActivityRequest req)
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
        _node.AddPendingTransaction(tx);
        return Ok("Transaction added");
    }

    private Transaction CreateUpdateGraphTransaction(Graph graph, string actor, string executingActivity)
    {
        var graphjson = JsonConvert.SerializeObject(graph);
        var graph_passbyvalue = JsonConvert.DeserializeObject<Graph>(graphjson)!;
        var updatedGraph = _graphExecutor.Execute(graph_passbyvalue, executingActivity);
        return new Transaction(actor, Action.Update, executingActivity, updatedGraph);
    }
}