using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.SignalR;

namespace DCR;
[ApiController]
[Route("[controller]")]
public class BlockchainController : ControllerBase
{

    private readonly ILogger<BlockchainController> _logger;
    private readonly NetworkClient _networkClient;
    private readonly AbstractNode _node;
    private readonly BlockchainSerializer _blockchainSerializer;
    private readonly IHubContext<BlockHub> _blockHubContext;

    public BlockchainController(ILogger<BlockchainController> logger, NetworkClient networkClient, AbstractNode node, IHubContext<BlockHub> blockHubContext)
    {
        _logger = logger;
        _node = node;
        _networkClient = networkClient;
        _blockchainSerializer = new BlockchainSerializer();
        _blockHubContext = blockHubContext;
    }

    [HttpGet("full")]
    public IActionResult GetBlockchain()
    {
        string blockchainJson = _blockchainSerializer.Serialize(_node.Blockchain);
        return Ok(blockchainJson);
    }

    [HttpGet("head")]
    public IActionResult GetHeadBlock()
    {
        return Ok(_blockchainSerializer.Serialize(_node.Blockchain.GetHead()));
    }

    [HttpGet("{index}")]
    public IActionResult GetBlock(int index)
    {
        try 
        {
            return Ok(_blockchainSerializer.Serialize(_node.Blockchain.Chain[index]));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return NotFound("Could not find block");
        }
    }

    [HttpPost("block")]
    public IActionResult ReceiveBlock(ShareBlockRequest req)
    {
        _node.ReceiveBlock(req.SourceNode, req.Block);
        _blockHubContext.Clients.All.SendAsync("update", _blockchainSerializer.Serialize(req.Block));
        return Ok();
    }

    [HttpPost("transaction")]
    public IActionResult ReceiveTransaction(Transaction transaction)
    {
        Console.WriteLine($"Received Transaction {transaction.Id}");
        _node.HandleTransaction(transaction);
        return Ok();
    }
}