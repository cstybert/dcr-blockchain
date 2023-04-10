using Microsoft.AspNetCore.Mvc;


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
        return Ok(_node.Blockchain.Chain);
    }

    [HttpGet("head")]
    public IActionResult GetHeadBlock()
    {
        return Ok(_node.Blockchain.GetHead());
    }
    [HttpGet("{index}")]
    public IActionResult GetBlock(int index)
    {
        try 
        {
            return Ok(_node.Blockchain.Chain[index]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return NotFound("Could not find block");
        }
    }

    [HttpPost("block")]
    public IActionResult ReceiveBlock(ShareBlock req)
    {
        Console.WriteLine($"Received block {req.Block.Hash}");
        _node.ReceiveBlock(req);

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