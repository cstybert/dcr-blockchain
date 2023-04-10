using Microsoft.Extensions.Options;
namespace DCR;

public class FullNode : AbstractNode
{
    private readonly ILogger<FullNode> _logger;
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    public FullNode(ILogger<FullNode> logger, IOptions<BlockchainSettings> settings, NetworkClient networkClient) 
    : base(settings, networkClient)
    {
        _logger = logger;
    }


    public override void HandleTransaction(Transaction tx)
    {
        Console.WriteLine("Handle transaction");
    }
}