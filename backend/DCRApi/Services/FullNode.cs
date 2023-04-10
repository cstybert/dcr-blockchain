namespace DCR;

public class FullNode : AbstractNode
{
    private readonly ILogger<FullNode> _logger;
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    public NetworkClient NetworkClient { get; }

    public FullNode(ILogger<FullNode> logger, NetworkClient networkClient)
    {
        _logger = logger;
        NetworkClient = networkClient;
        // When we add network we should not create a blockchain.json from scract
        // but instead ask neighbors for the up to date version, so this should be removed and replaced
        // by below comment:
        if (!System.IO.File.Exists("blockchain.json"))
        {
            // TODO: Query neighbors
        }
        else
        {
            var blockJson = System.IO.File.ReadAllText("blockchain.json");
            Blockchain = _blockchainSerializer.Deserialize(blockJson);
        }

        // REPLACE WITH THIS
        // if (System.IO.File.Exists("blockchain.json"))
        // {
        //     string blockJson = System.IO.File.ReadAllText("blockchain.json");
        //     Blockchain = _blockchainSerializer.Deserialize(blockJson);
        // }
    }


    // ------------------------------------------------------------------------------------------------------------
    // Just an idea for the implementations

    // Step 1  : If block has same index or lower than local head, ignore it.
    // Step 2  : Check if block is valid and ignore if not.
    // Step 3  : call ResyncLarger()
    public override void ReceiveBlock(Block block) // TODO: Add sender parameter?
    {
        if (block.Index <= Blockchain.GetHead().Index)
        {
            return;
        }
        if (!block.IsValid(Blockchain.Difficulty))
        {
            return;
        }
        // ResyncLarger(sender, block); //TODO: How to communicate back to sender?
    }

    // ------------------------------------------------------------------------------------------------------------

    public override void HandleTransaction(Transaction tx)
    {
    }
}