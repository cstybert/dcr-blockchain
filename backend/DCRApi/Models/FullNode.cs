namespace DCR;

public class FullNode : AbstractNode
{
    private readonly ILogger<FullNode> _logger;
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    public FullNode(ILogger<FullNode> logger, NetworkClient networkClient): base(networkClient)
    {
        _logger = logger;
    }

    public override void HandleTransaction(Transaction tx)
    {
        if (IsValidTransaction(tx))
        {
            lock (Blockchain)
            {
                if (!Blockchain.Chain.Any(b => b.Transactions.Any(t => t.Id == tx.Id)))
                {
                    NetworkClient.BroadcastTransaction(tx);
                }
            }
        }
    }
}