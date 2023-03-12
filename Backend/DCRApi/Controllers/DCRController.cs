using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        BlockChain blockchain = new BlockChain(2);
        List<Transaction> tx = new List<Transaction>(){new Transaction("Foo", Action.Create, "Bar")};
        Block newblock = new Block(tx);
        blockchain.AddBlock(newblock);
        Block newblock2 = new Block(new List<Transaction>());
        blockchain.AddBlock(newblock2);
        Console.WriteLine($"Block validity: {blockchain.IsValid()}");
        return JsonConvert.SerializeObject(blockchain);
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