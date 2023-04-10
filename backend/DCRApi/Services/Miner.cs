using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace DCR;

public class Miner : AbstractNode
{
    private readonly ILogger<Miner> _logger;
    private readonly ConcurrentQueue<Transaction> _queue = new ConcurrentQueue<Transaction>();
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();

    public Miner(ILogger<Miner> logger, IOptions<BlockchainSettings> settings, NetworkClient networkClient) 
    : base(settings, networkClient)
    {
        _logger = logger;
    }

    public void CancelMining()
    {
        miningCTSource.Cancel();
    }

    public void Mine()
    {
        CancellationToken miningCT = miningCTSource.Token;
        List<Transaction> txs = new List<Transaction>();
        Transaction? transaction;
        int i = 0;
        while (i < Settings.SizeOfBlocks) // Blocks contain 10 transactions
        {
            _queue.TryDequeue(out transaction);
            if (transaction is null)
            {
                break;
            }
            else
            {
                if (!Blockchain.Chain.Any(b => b.Transactions.Any(t => t.Id == transaction.Id)))
                {
                    txs.Add(transaction);
                    i++;
                }
            }
        }
        var newBlock = Blockchain.MineTransactions(txs, miningCT);
        if (!miningCT.IsCancellationRequested)
        {
            // ShareBlock(newBlock);
            Save();
        }
        if (miningCT.IsCancellationRequested)
        {
            Console.WriteLine("Cancellation was requested");
            miningCTSource = new CancellationTokenSource();
        }
    }
    public override void HandleTransaction(Transaction tx)
    {
        if (!_queue.Any(t => t.Id == tx.Id))
        {
            _queue.Enqueue(tx);
        }
    }
}