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

    public Miner(ILogger<Miner> logger, NetworkClient networkClient): base(networkClient)
    {
        _logger = logger;
    }

    public override void HandleTransaction(Transaction tx)
    {
        lock (_queue)
        {
            if (!_queue.Any(t => t.Id == tx.Id))
            {
                _queue.Enqueue(tx);
            }
        }
    }

    public void Mine()
    {
        CancellationToken miningCT = miningCTSource.Token;
        List<Transaction> txs = new List<Transaction>();
        Transaction? transaction;
        int i = 0;
        Console.WriteLine("d");
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Console.WriteLine("c");
        while (stopwatch.ElapsedMilliseconds < 4000 && i < Settings.SizeOfBlocks)
        {
            Console.WriteLine($"Elapsed : {stopwatch.ElapsedMilliseconds}");
            _queue.TryDequeue(out transaction);
            if (transaction is null)
            {
                break;
            }
            else
            {
                if (!Blockchain.Chain.Any(b => b.Transactions.Any(t => t.Id == transaction.Id)))
                {
                    // Check transaction validity if doing evaluation with lazy evaluation
                    if (IsValidTransaction(transaction))
                    {
                        txs.Add(transaction);
                        i++;
                    }
                }
            }
        }
        Console.WriteLine($"Spent {stopwatch.Elapsed} ms mining");
        stopwatch.Stop();
        Thread.Sleep(Settings.TimeToSleep); // For testing, a block is added every 15 seconds
        var newBlock = Blockchain.MineTransactions(txs, miningCT);
        if (!miningCT.IsCancellationRequested)
        {
            if (!Settings.IsEval) ShareBlock(newBlock);
            Save();
        }
        if (miningCT.IsCancellationRequested)
        {
            Console.WriteLine("Cancellation was requested");
            miningCTSource = new CancellationTokenSource();
        }
        if(Settings.IsEval)
        {
            return;
        } 
    }

    public void CancelMining()
    {
        miningCTSource.Cancel();
    }
}