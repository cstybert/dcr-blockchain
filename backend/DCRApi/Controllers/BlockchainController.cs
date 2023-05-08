using Microsoft.AspNetCore.Mvc;

namespace DCR;
[ApiController]
[Route("[controller]")]
public class BlockchainController : ControllerBase
{

    private readonly ILogger<BlockchainController> _logger;
    private readonly AbstractNode _node;
    private readonly BlockchainSerializer _blockchainSerializer;

    public BlockchainController(ILogger<BlockchainController> logger, AbstractNode node)
    {
        _logger = logger;
        _node = node;
        _blockchainSerializer = new BlockchainSerializer();
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
        if (req.Block.Transactions.Any()) {
            Console.WriteLine($"Received Block {req.Block.Hash}");
        }
        _node.ReceiveBlock(req.SourceNode, req.Block);

        return Ok();
    }

    [HttpPost("transaction")]
    public IActionResult ReceiveTransaction(Transaction transaction)
    {
        Console.WriteLine($"Received Transaction {transaction.Id}");
        if (!_node.HandledTransactions.Any(tx => tx.Id == transaction.Id))
        {
            _node.HandleTransaction(transaction);
            _node.HandledTransactions.Add(transaction);
        } else {
            Console.WriteLine("Transaction has already been handled (ignore).");
        }
        return Ok();
    }
}