using Microsoft.AspNetCore.Mvc;


namespace DCR;
[ApiController]
[Route("[controller]")]
public class BlockchainController : ControllerBase
{

    private readonly ILogger<BlockchainController> _logger;
    private readonly NetworkClient _networkClient;
    private readonly Miner _miner;
    private readonly BlockchainSerializer _blockchainSerializer;

    public BlockchainController(ILogger<BlockchainController> logger, NetworkClient networkClient, Miner miner)
    {
        _logger = logger;
        _miner = miner;
        _networkClient = networkClient;
        _blockchainSerializer = new BlockchainSerializer();
    }

    [HttpGet("full")]
    public IActionResult GetBlockchain()
    {
        return Ok(_miner.Blockchain.Chain);
    }

    [HttpGet("head")]
    public IActionResult GetHeadBlock()
    {
        return Ok(_miner.Blockchain.GetHead());
    }

    [HttpPost("block")]
    public IActionResult ReceiveBlock(Block receivedBlock)
    {
        Console.WriteLine($"Received block {receivedBlock.Hash}");
        if (!(receivedBlock.Index <= _miner.Blockchain.GetHead().Index)) {
            _miner.Blockchain.AddBlock(receivedBlock);
        }
        return Ok();
    }
}