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

    [HttpPost("block")]
    public IActionResult ReceiveBlock(Block receivedBlock)
    {
        Console.WriteLine($"Received block {receivedBlock.Hash}");
        _node.ReceiveBlock(receivedBlock);

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