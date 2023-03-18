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
    private BlockChain _blockchain;
    private BlockChainSerializer _blockChainSerializer;
    private GraphSerializer _graphSerializer;
    private GraphCreator _graphCreator;
    private GraphExecutor _graphExecutor;

    public DCRController(ILogger<DCRController> logger)
    {
        _graphSerializer = new GraphSerializer();
        _blockChainSerializer = new BlockChainSerializer();
        _graphCreator = new GraphCreator();
        _graphExecutor = new GraphExecutor();
        _logger = logger;
        _blockchain = new BlockChain(2);
        if (!System.IO.File.Exists("blockchain.json"))
        {
            _blockchain = new BlockChain(2);
            _blockchain.Save();
        }
        else
        {
            var blockJson = System.IO.File.ReadAllText("blockchain.json");
            _blockchain = _blockChainSerializer.Deserialize(blockJson);
        }
    }

    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        _logger.LogTrace($"Fetching graph {id}");
        return Ok(_blockchain.GetGraph(id));
    }
    
    [HttpPost("create")]
    public IActionResult Post(CreateGraph req)
    {
        _logger.LogTrace("Adding graph to blockchain");

        var graph = _graphCreator.Create(req.Graph.Activities, req.Graph.Relations);
        var tx = new Transaction(req.Actor, Action.Create, graph);
        _blockchain.AddBlock(tx);
        
        Console.WriteLine($"Block validity: {_blockchain.IsValid()}");
        _logger.LogTrace($"Added graph {graph.ID}");
        return Ok(graph);
    }

    [HttpPut("update/{id}")]
    public IActionResult Put(string id, ExecuteActivity req)
    {
        _logger.LogTrace($"Executing activity {req.ExecutingActivity} in graph {id}");
        var graph = _blockchain.GetGraph(id);
        var updatedGraph = _graphExecutor.Execute(graph, req.ExecutingActivity);
        var transaction = new Transaction(req.Actor, Action.Update, updatedGraph);
        _blockchain.AddBlock(transaction);

        _logger.LogTrace($"Updated graph {graph.ID}");
        return Ok(updatedGraph);
    }
}