using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace DCR;

public interface IMiner
{
    public void Mine();
    public void CancelMining();
}
public class Miner :  AbstractNode, IMiner
{
    private readonly ILogger<Miner> _logger;
    private readonly ConcurrentQueue<Transaction> _queue = new ConcurrentQueue<Transaction>();

    public Miner(ILogger<Miner> logger, NetworkClient networkClient, Settings settings): base(networkClient, settings)
    {
        _logger = logger;
    }

    public override void HandleTransaction(Transaction tx)
    {
        lock (_queue)
        {
            if (!Blockchain.Chain.Any(b => b.Transactions.Any(t => t.Id == tx.Id)))
            {
                if (!_queue.Any(t => t.Id == tx.Id))
                {
                    _queue.Enqueue(tx);
                    HandledTransactions.Add(tx);
                }
            }
        }
    }

    public void Mine()
    {
        var miningCT = miningCTSource.Token;
        var txs = DequeueTransactions(miningCT);
        Thread.Sleep(_settings.TimeToSleep);
        MineTransactions(txs, miningCT);
    }

    public List<Transaction> DequeueTransactions(CancellationToken miningCT) {
        List<Transaction> txs = new List<Transaction>();
        Transaction? transaction;
        int i = 0;
        while (i < _settings.SizeOfBlocks)
        {
            if (miningCT.IsCancellationRequested) {
                return txs;
            }
            _queue.TryDequeue(out transaction);
            if (transaction is null)
            {
                break;
            }
            else
            {
                // Check transaction validity if doing evaluation with lazy evaluation
                if (IsValidTransaction(transaction, txs))
                {
                    txs.Add(transaction);
                    i++;
                }
            }
        }
        return txs;
    }

    private void MineTransactions(List<Transaction> txs, CancellationToken miningCT) {
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