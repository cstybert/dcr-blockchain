using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace DCR;

public class Miner : AbstractNode
{
    private readonly ILogger<Miner> _logger;
    public override Blockchain Blockchain {get; init;}
    private readonly ConcurrentQueue<Transaction> _queue = new ConcurrentQueue<Transaction>();
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    public override NetworkClient NetworkClient { get; init; }
    public BlockchainSettings Settings { get; }

    public Miner(ILogger<Miner> logger, IOptions<BlockchainSettings> settings, NetworkClient networkClient)
    {
        _logger = logger;
        Settings = settings.Value;
        NetworkClient = networkClient;
        // When we add network we should not create a blockchain.json from scract
        // but instead ask neighbors for the up to date version, so this should be removed and replaced
        // by below comment:
        if (!System.IO.File.Exists("blockchain.json"))
        {
            Blockchain? blockchain = NetworkClient.GetBlockchain().Result;
            CancellationToken mineCT = miningCTSource.Token;
            if (blockchain is not null)
            {
                Blockchain = blockchain;
            }
            else
            {
                Blockchain = new Blockchain(Settings.Difficulty);
                Blockchain.Initialize(mineCT);
            }
        }
        else
        {
            var blockJson = System.IO.File.ReadAllText("blockchain.json");
            Blockchain = _blockchainSerializer.Deserialize(blockJson);
            ResyncBlockchain(Settings.NumberNeighbours);
        }
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
            ShareBlock(newBlock);
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