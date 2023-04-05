using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Models;
using Business;

namespace DCR;
[ApiController]
[Route("[controller]")]
public class DCRController : ControllerBase
{

    private readonly ILogger<DCRController> _logger;
    private readonly Node _node;
    private readonly BlockChainSerializer _blockChainSerializer = new BlockChainSerializer();
    private readonly GraphSerializer _graphSerializer = new GraphSerializer();
    private readonly GraphCreator _graphCreator = new GraphCreator();
    private readonly GraphExecutor _graphExecutor = new GraphExecutor();

    public DCRController(ILogger<DCRController> logger, Node node)
    {
        _node = node;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        _logger.LogTrace($"Fetching graph {id}");
        _logger.LogInformation($"Block validity: {_node.Blockchain.IsValid()}");
        // TODO Modify getgraph so it only returns if graph is 8 blocks deep
        Graph graph = _node.Blockchain.GetGraph(id)!;
        if (graph is not null)
        {
            return Ok(_node.Blockchain.GetGraph(id));
        }
        return NotFound("Could not find graph");
    }
    
    //test endpoint
    [HttpPost("cancel")]
    public IActionResult cancel()
    {
        _node.CancelMining();
        return Ok("Mining cancelled");
    }

    [HttpPost("create")]
    public IActionResult Post([FromBody] CreateGraph req)
    {
        _logger.LogInformation($"Adding graph to blockchain : {req}");
        Graph graph = _graphCreator.Create(req.Activities, req.Relations);
        Transaction tx = new Transaction(req.Actor, Action.Create, graph);
        _node.AddTransaction(tx);
        _logger.LogInformation($"Block validity: {_node.Blockchain.IsValid()}");
        _logger.LogInformation($"Created Transaction");
        return Ok("Transaction added");
    }

    [HttpPut("update/{id}")]
    public IActionResult Put(string id, ExecuteActivity req)
    {
        _logger.LogInformation($"Executing activity {req.ExecutingActivity} in graph {id}");
        Graph graph = _node.Blockchain.GetGraph(id)!;
        if (graph is null)
        {
            return NotFound("Could not find graph");
        }
        Transaction tx = CreateUpdateGraphTransaction(graph, req.Actor, req.ExecutingActivity);
        _node.AddTransaction(tx);
        _logger.LogInformation($"Block validity: {_node.Blockchain.IsValid()}");
        _logger.LogInformation($"Updated graph {graph.ID}");
        return Ok("Transaction added");
    }

    private Transaction CreateUpdateGraphTransaction(Graph Graph, string Actor, string ExecutingActivity)
    {
        string graphjson = JsonConvert.SerializeObject(Graph);
        Graph graph_passbyvalue = JsonConvert.DeserializeObject<Graph>(graphjson)!;
        Graph updatedGraph = _graphExecutor.Execute(graph_passbyvalue, ExecutingActivity);
        return new Transaction(Actor, Action.Update, updatedGraph);
    }
}