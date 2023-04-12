using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DCR;
[ApiController]
[Route("[controller]")]
public class BlockchainController : ControllerBase
{

    private readonly ILogger<BlockchainController> _logger;
    private readonly NetworkClient _networkClient;
    private readonly AbstractNode _node;
    private readonly BlockchainSerializer _blockchainSerializer;

    public BlockchainController(ILogger<BlockchainController> logger, NetworkClient networkClient, AbstractNode node)
    {
        _logger = logger;
        _node = node;
        _networkClient = networkClient;
        _blockchainSerializer = new BlockchainSerializer();
    }

    [HttpGet("full")]
    public IActionResult GetBlockchain()
    {
        _logger.LogDebug($"Received request for full blockchain");
        _logger.LogInformation($"Received request for full blockchain");
        string blockchainJson = _blockchainSerializer.Serialize(_node.Blockchain);
        return Ok(blockchainJson);
    }

    [HttpGet("head")]
    public IActionResult GetHeadBlock()
    {
        _logger.LogDebug($"Received Head request");
        _logger.LogInformation($"Received Head request");
        return Ok(_blockchainSerializer.Serialize(_node.Blockchain.GetHead()));
    }

    [HttpGet("{index}")]
    public IActionResult GetBlock(int index)
    {
        _logger.LogDebug($"Received Block request {index}");
        _logger.LogInformation($"Received Block request {index}");
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
        _logger.LogDebug($"Received Block {req.Block.Hash}");
        _logger.LogInformation($"Received Block {req.Block.Hash}");
        _node.ReceiveBlock(req.SourceNode, req.Block);
        return Ok();
    }

    [HttpPost("transaction")]
    public IActionResult ReceiveTransaction(Transaction transaction)
    {
        _logger.LogDebug($"Received Transaction {transaction.Id}");
        _logger.LogInformation($"Received Transaction {transaction.Id}");
        _node.HandleTransaction(transaction);
        return Ok();
    }
}