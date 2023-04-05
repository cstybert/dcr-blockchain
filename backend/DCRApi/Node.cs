using System.Collections.Concurrent;
namespace DCR;

public class Node : BackgroundService
{
    private readonly ILogger<Node> _logger;
    private readonly ConcurrentQueue<Transaction> _queue = new ConcurrentQueue<Transaction>();
    public BlockChain Blockchain {get; init;}
    private readonly BlockChainSerializer _blockChainSerializer = new BlockChainSerializer();
    CancellationTokenSource miningCTSource = new CancellationTokenSource();

    public Node(ILogger<Node> logger)
    {
        _logger = logger;
        // When we add network we should not create a blockchain.json from scract
        // but instead ask neighbors for the up to date version, so this should be removed and replaced
        // by below comment:
        if (!System.IO.File.Exists("blockchain.json"))
        {
            CancellationToken mineCT = miningCTSource.Token;
            Blockchain = new BlockChain(3);
            Blockchain.Initialize(mineCT);
        }
        else
        {
            var blockJson = System.IO.File.ReadAllText("blockchain.json");
            Blockchain = _blockChainSerializer.Deserialize(blockJson);
        }

        // REPLACE WITH THIS
        // if (System.IO.File.Exists("blockchain.json"))
        // {
        //     string blockJson = System.IO.File.ReadAllText("blockchain.json");
        //     Blockchain = _blockChainSerializer.Deserialize(blockJson);
        // }
    }


    // ------------------------------------------------------------------------------------------------------------
    // Just an idea for the implementations

    private void ResyncBlockchain()
    {
        throw new NotImplementedException();
        // Step 0  : If Blockchain is null call GetBlockchain() and return;
        // step 1  : Ask neighbor for head of their local blockchain 
        // step 1a : (if index of remote block is smaller than local head then ask another n neighbors?)
        // Step 2  : If block has same index as local head, return.
        // Step 3  : Check if block is valid and ignore if not.
        // Step 5  : call ResyncLarger()
    }

    private void ResyncLarger() // figure out better name
    {
    // Step 1  : If remote block is valid and has index = local head index + 1, and previoushash equal to local head
    //           cancel local mining task , add the remote block, and share the new local head.
    // Step 2  : Else if remote block is valid: save current length of blockchain in a variable "local_chain_length"
    // Step 2  : Check if previousHash on remote equals hash of a block in local chain.
    //           If not, request the previous block from remote and continue to check if previoushash equals a 
    //           hash in local chain. Continue until a match is found or all blocks have been requested,
    //           compare the length of replacing local chain with the remote from the matching hash onwards with "local_chain_length"
    //           if the length of replacing is longer, cancel mining task and do it. Else dont.
    //           Call ShareBlock() with new head. 
    //           If an invalid block is encountered at any time, the process should be aborted.
    }

    private void GetBlockchain()
    {
        throw new NotImplementedException();
        // Ask neighbor for entire blockchain
    }

    private void ShareBlock(Block block)
    {
        throw new NotImplementedException();
        // Send newly mined block to all neighbours
    }

    private void ReceiveBlock(string sender, Block block)
    {
        throw new NotImplementedException();
        // Step 1  : If block has same index or lower than local head, ignore it.
        // Step 2  : Check if block is valid and ignore if not.
        // Step 3  : call ResyncLarger()

    }

    // ------------------------------------------------------------------------------------------------------------

    public override async Task StartAsync(CancellationToken stoppingToken)
    {
        // DiscoverNetwork();
        // ResyncBlockchain();
        _logger.LogInformation("Starting Node");
        await base.StartAsync(stoppingToken);
    }
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        Blockchain.Save();
        // GoOffline();
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
            Thread.Sleep(5000);
        }
    }

    public void CancelMining()
    {
        miningCTSource.Cancel();
    }

    private void Mine()
    {
        CancellationToken mineCT = miningCTSource.Token;
        List<Transaction> txs = new List<Transaction>();
        Transaction? transaction;
        for (int i = 0; i < 10; i++) // Blocks contain 10 transactions
        {
            _queue.TryDequeue(out transaction);
            if (transaction is null)
            {
                break;
            }
            else
            {
                txs.Add(transaction);
            }
        }
        Blockchain.AddBlock(txs, mineCT);
        if (!mineCT.IsCancellationRequested)
        {
            Console.WriteLine("Should share");
        }
        if (mineCT.IsCancellationRequested)
        {
            Console.WriteLine("Cancellation was requested");
            miningCTSource.TryReset();
        }
        // if task is not cancelled call ShareBlock();
    }
    public void AddTransaction(Transaction tx)
    {
        _queue.Enqueue(tx);
    }
}