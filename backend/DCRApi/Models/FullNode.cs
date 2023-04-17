using Models;

namespace DCR;

public class FullNode : AbstractNode
{
    private readonly ILogger<FullNode> _logger;
    private readonly BlockSerializer _blockSerializer;
    public List<Graph> DiscoveredGraphs { get; set; }
    public List<Transaction> PendingTransactions { get; set; }
    public FullNode(ILogger<FullNode> logger, NetworkClient networkClient): base(networkClient)
    {
        _logger = logger;
        _blockSerializer = new BlockSerializer();
        DiscoveredGraphs = new List<Graph>();
        PendingTransactions = new List<Transaction>();
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

    public override bool ReceiveBlock(NetworkNode sender, Block receivedBlock) {
        if (base.ReceiveBlock(sender, receivedBlock))
        {
            if (receivedBlock.Transactions.Any()) {
                PendingTransactions.RemoveAll(pt => receivedBlock.Transactions.Any(t => t.Id == pt.Id));
                PendingTransactionsHub.SendUpdateNotification();
            }
            return true;
        }
        return false;
    }

    public void AddDiscoveredGraph(Graph graph)
    {
        if (!DiscoveredGraphs.Any(g => g.Id == graph.Id))
        {
            DiscoveredGraphs.Add(graph);
        }
    }

    public void AddPendingTransaction(Transaction tx)
    {
        if (!PendingTransactions.Any(t => t.Id == tx.Id))
        {
            PendingTransactions.Add(tx);
        }
    }
}