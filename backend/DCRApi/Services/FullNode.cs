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
        Random r = new Random();
        int name = r.Next();
        if (!System.IO.File.Exists($"blockchain{Id}.json"))
        {
            Console.WriteLine("Getting blockchain");
            Blockchain? blockchain = NetworkClient.GetBlockchain().Result;
            Console.WriteLine("Got blockchain");
            CancellationToken mineCT = miningCTSource.Token;
            if (blockchain is not null)
            {
                Console.WriteLine("Blockchain was not null");
                Blockchain = blockchain;
                Save();
            }
            else
            {
                Console.WriteLine("Blockchain was null");
                Blockchain = new Blockchain(Settings.Difficulty);
                Blockchain.Initialize(mineCT);
                Save();
            }
        }
        else
        {
            var blockJson = System.IO.File.ReadAllText($"blockchain{Id}.json");
            Blockchain = _blockchainSerializer.Deserialize(blockJson);
            ResyncBlockchain(Settings.NumberNeighbours);
        }
    }


    public override void HandleTransaction(Transaction tx)
    {
        Console.WriteLine("Handle transaction");
    }
}