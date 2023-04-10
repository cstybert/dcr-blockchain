using Microsoft.Extensions.Options;
namespace DCR;

public class FullNode : AbstractNode
{
    private readonly ILogger<FullNode> _logger;
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    public FullNode(ILogger<FullNode> logger, NetworkClient networkClient) 
    : base(networkClient)
    {
        _logger = logger;
    }

    public override void Mine()
    {
        throw new NotImplementedException();
    }


    public override void HandleTransaction(Transaction tx)
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