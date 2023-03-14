using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DCR;
[ApiController]
[Route("[controller]")]
public class DCRController : ControllerBase
{

    private readonly ILogger<DCRController> _logger;
    private BlockChain _blockchain;

    public DCRController(ILogger<DCRController> logger)
    {
        _logger = logger;
        if (!System.IO.File.Exists("blockchain.json"))
        {
            _blockchain = new BlockChain(2);
            using (StreamWriter sw = System.IO.File.CreateText("blockchain.json"))
            {
                sw.Write(JsonConvert.SerializeObject(_blockchain));
            }
        }
        else
        {
            string blockjson = System.IO.File.ReadAllText("blockchain.json");
            _blockchain = JsonConvert.DeserializeObject<BlockChain>(blockjson)!;
        }
    }

    [HttpGet("{id}")]
    public string Get(string id)
    {
        _logger.LogTrace("Tried getting the DCR id");
        return _blockchain.GetGraph(id);
    }
    
    [HttpPost("CreateGraph")]
    public string Post(CreateGraph req)
    {
        _logger.LogTrace("Tried posting a DCR Graph");
        Transaction tx = new Transaction(req.Actor, Action.Create, req.Graph);
        List<Transaction> txl = new List<Transaction>(){tx};
        Block newblock = new Block(txl);
        _blockchain.AddBlock(newblock);
        using (StreamWriter sw = System.IO.File.CreateText("blockchain.json"))
        {
            sw.Write(JsonConvert.SerializeObject(_blockchain));
        }
        Console.WriteLine($"Block validity: {_blockchain.IsValid()}");
        return JsonConvert.SerializeObject(_blockchain);
    }

    [HttpPost("updategraph/{id}")]
    public string Update(string id)
    {
        _logger.LogTrace("Tried updating the DCR id");
        return $"Tried updating id : {id}";
    }
}