using Microsoft.Extensions.Options;
namespace DCR;

public class FullNode : AbstractNode
{
    private readonly ILogger<FullNode> _logger;
    public override Blockchain Blockchain {get; init;}
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    public override NetworkClient NetworkClient { get; init; }
    public BlockchainSettings Settings { get; }
    public FullNode(ILogger<FullNode> logger, IOptions<BlockchainSettings> settings, NetworkClient networkClient)
    {
        _logger = logger;
        Settings = settings.Value;
        NetworkClient = networkClient;
        // When we add network we should not create a blockchain.json from scract
        // but instead ask neighbors for the up to date version, so this should be removed and replaced
        // by below comment:
        if (!System.IO.File.Exists("blockchain.json"))
        {
            Blockchain? blockchain = NetworkClient.GetBlockchain().Result;
            CancellationToken mineCT = miningCTSource.Token;
            if (blockchain is not null)
            {
                Blockchain = blockchain;
            }
            else
            {
                Blockchain = new Blockchain(Settings.Difficulty);
                Blockchain.Initialize(mineCT);
            }
        }
        else
        {
            var blockJson = System.IO.File.ReadAllText("blockchain.json");
            Blockchain = _blockchainSerializer.Deserialize(blockJson);
            ResyncBlockchain(Settings.NumberNeighbours);
        }
    }


    public override void HandleTransaction(Transaction tx)
    {
    }
}