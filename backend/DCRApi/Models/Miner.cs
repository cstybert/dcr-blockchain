using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace DCR;

public class Miner : AbstractNode
{
    private readonly ILogger<Miner> _logger;
    private readonly ConcurrentQueue<Transaction> _queue = new ConcurrentQueue<Transaction>();

    public Miner(ILogger<Miner> logger, NetworkClient networkClient): base(networkClient)
    {
        _logger = logger;
    }

    public override void HandleTransaction(Transaction tx)
    {
        if (IsValidTransaction(tx)) {
            lock (_queue)
            {
                if (!_queue.Any(t => t.Id == tx.Id))
                {
                    _queue.Enqueue(tx);
                }
            }
        }
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
        Thread.Sleep(Settings.TimeToSleep); // For testing, a block is added every 15 seconds
        var newBlock = Blockchain.MineTransactions(txs, miningCT);
        if (!miningCT.IsCancellationRequested)
        {
            ShareBlock(newBlock);
            Save();
        }
        if (miningCT.IsCancellationRequested)
        {
            Console.WriteLine("Cancellation was requested");
            miningCTSource = new CancellationTokenSource();
        }
    }

    public void CancelMining()
    {
        miningCTSource.Cancel();
    }
}