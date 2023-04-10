using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace DCR;

public class Miner : AbstractNode
{
    private readonly ILogger<Miner> _logger;
    private readonly ConcurrentQueue<Transaction> _queue = new ConcurrentQueue<Transaction>();
    public override Blockchain Blockchain {get; init;}
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    private CancellationTokenSource miningCTSource = new CancellationTokenSource();
    public override NetworkClient NetworkClient {get; init;}
    private readonly MinerSettings _settings;

    public Miner(ILogger<Miner> logger, IOptions<MinerSettings> settings, NetworkClient networkClient)
    {
        _logger = logger;
        _settings = settings.Value;
        NetworkClient = networkClient;
        // When we add network we should not create a blockchain.json from scract
        // but instead ask neighbors for the up to date version, so this should be removed and replaced
        // by below comment:
        if (!System.IO.File.Exists("blockchain.json"))
        {
            Blockchain? blockchain = NetworkClient.GetBlockchain();
            CancellationToken mineCT = miningCTSource.Token;
            Blockchain = new Blockchain(_settings.Difficulty);
            Blockchain.Initialize(mineCT);
        }
        else
        {
            var blockJson = System.IO.File.ReadAllText("blockchain.json");
            Blockchain = _blockchainSerializer.Deserialize(blockJson);
            ResyncBlockchain(_settings.NumberNeighbours);
        }

        // REPLACE WITH THIS
        // if (System.IO.File.Exists("blockchain.json"))
        // {
        //     string blockJson = System.IO.File.ReadAllText("blockchain.json");
        //     Blockchain = _blockchainSerializer.Deserialize(blockJson);
        // }
    }

    public override async Task StartAsync(CancellationToken stoppingToken)
    {
        NetworkClient.DiscoverNetwork();
        _logger.LogInformation("Starting Node");
        await base.StartAsync(stoppingToken);
    }
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        Blockchain.Save();
        await NetworkClient.DisconnectFromNetwork();
        _logger.LogInformation($"Stopping Node");
        await base.StopAsync(stoppingToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Task task = new Task(new System.Action(Mine));
            task.Start();
            await task;
            Thread.Sleep(_settings.TimeToSleep); // For testing, a block is added every 15 seconds
        }
    }

    public void CancelMining()
    {
        miningCTSource.Cancel();
    }

    private void Mine()
    {
        CancellationToken miningCT = miningCTSource.Token;
        List<Transaction> txs = new List<Transaction>();
        Transaction? transaction;
        int i = 0;
        while (i < _settings.SizeOfBlocks) // Blocks contain 10 transactions
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
    public void AddTransaction(Transaction tx)
    {
        if (!_queue.Any(t => t.Id == tx.Id))
        {
            _queue.Enqueue(tx);
        }
    }
}