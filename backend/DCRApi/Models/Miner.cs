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
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        while (stopwatch.ElapsedMilliseconds < 4000 && i < _settings.SizeOfBlocks)
        {
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
        Console.WriteLine($"Spent {stopwatch.Elapsed} ms mining");
        stopwatch.Stop();
        Thread.Sleep(_settings.TimeToSleep); // For testing, a block is added every 15 seconds
        var newBlock = Blockchain.MineTransactions(txs, miningCT);
        if (!miningCT.IsCancellationRequested)
        {
            if (!_settings.IsEval) ShareBlock(newBlock);
            Save();
        }
        if (miningCT.IsCancellationRequested)
        {
            Console.WriteLine("Cancellation was requested");
            miningCTSource = new CancellationTokenSource();
        }
        if(_settings.IsEval)
        {
            return;
        } 
    }

    public void CancelMining()
    {
        miningCTSource.Cancel();
    }
}