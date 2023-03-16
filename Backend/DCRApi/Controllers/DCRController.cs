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

    public DCRController(ILogger<DCRController> logger)
    {
        _graphSerializer = new GraphSerializer();
        _blockChainSerializer = new BlockChainSerializer();
        _logger = logger;
        _blockchain = new BlockChain(2);
        /* if (!System.IO.File.Exists("blockchain.json"))
        {
            _blockchain = new BlockChain(2);
            using (StreamWriter sw = System.IO.File.CreateText("blockchain.json"))
            {
                sw.Write(_blockChainSerializer.Serialize(_blockchain));
            }
        }
        else
        {
            string blockjson = System.IO.File.ReadAllText("blockchain.json");
            _blockchain = _blockChainSerializer.Deserialize(blockjson);
        } */
    }

    [HttpGet("{id}")]
    public string Get(string id)
    {
        _logger.LogTrace($"Tried getting the DCR {id}");
        return _blockchain.GetGraph(id);
    }
    
    [HttpPost("CreateGraph")]
    public string Post(CreateGraph req)
    {
        _logger.LogTrace("Tried posting a DCR Graph");

        var graphJson = _graphSerializer.Serialize(req.Graph);
        Transaction tx = new Transaction(req.Actor, Action.Create, graphJson);
        _blockchain.AddBlock(tx);
        
        var blockChainJson = _blockChainSerializer.Serialize(_blockchain);
        /* using (StreamWriter sw = System.IO.File.CreateText("blockchain.json"))
        {
            sw.Write(blockChainJson);
        } */
        Console.WriteLine($"Block validity: {_blockchain.IsValid()}");
        return blockChainJson;
    }

    [HttpPut("updategraph/{id}")]
    public string Put(ExecuteActivity req)
    {
        var dcrEngine = new GraphExecutor();
        var updatedGraph = dcrEngine.Execute(req.Graph, req.ExecutingActivity);
        var updatedGraphJson = _graphSerializer.Serialize(updatedGraph);
        var transaction = new Transaction(req.Actor, Action.Update, updatedGraphJson);
        _blockchain.AddBlock(transaction);

        _logger.LogTrace($"Updated state of graph {req.Graph.id}");
        return $"updatedGraphJson";
    }
}