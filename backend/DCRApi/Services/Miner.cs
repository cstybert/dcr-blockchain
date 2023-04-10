using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace DCR;

public class Miner : BackgroundService
{
    private readonly ILogger<Miner> _logger;
    private readonly ConcurrentQueue<Transaction> _queue = new ConcurrentQueue<Transaction>();
    public Blockchain Blockchain {get; init;}
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    private CancellationTokenSource miningCTSource = new CancellationTokenSource();
    private readonly NetworkClient _networkClient;
    private readonly MinerSettings _settings;

    public Miner(ILogger<Miner> logger, IOptions<MinerSettings> settings, NetworkClient networkClient)
    {
        _logger = logger;
        _settings = settings.Value;
        _networkClient = networkClient;
        // When we add network we should not create a blockchain.json from scract
        // but instead ask neighbors for the up to date version, so this should be removed and replaced
        // by below comment:
        if (!System.IO.File.Exists("blockchain.json"))
        {
            CancellationToken mineCT = miningCTSource.Token;
            Blockchain = new Blockchain(_settings.Difficulty);
            Blockchain.Initialize(mineCT);
        }
        else
        {
            var blockJson = System.IO.File.ReadAllText("blockchain.json");
            Blockchain = _blockchainSerializer.Deserialize(blockJson);
        }

        // REPLACE WITH THIS
        // if (System.IO.File.Exists("blockchain.json"))
        // {
        //     string blockJson = System.IO.File.ReadAllText("blockchain.json");
        //     Blockchain = _blockchainSerializer.Deserialize(blockJson);
        // }
    }


    // ------------------------------------------------------------------------------------------------------------
    // Just an idea for the implementations

    private void ResyncBlockchain()
    {
        // Step 0  : If Blockchain is null call GetBlockchain() and return;
        // step 1  : Ask neighbor for head of their local blockchain 
        // step 1a : (if index of remote block is smaller than local head then ask another n neighbors?)
        // Step 2  : If block has same index as local head, return.
        // Step 3  : Check if block is valid and ignore if not.
        // Step 5  : call ResyncLarger()
        throw new NotImplementedException();
        if (Blockchain is null) 
        {
            GetBlockchain();
            return;
        }
        for (int i = 0; i < _settings.NumberNeighbours; i++)
        {
            Block block = GetHeadFromNeighbour();
            if (block.Index == Blockchain.GetHead().Index)
            {
                return;
            }
            if ((block.Index > Blockchain.GetHead().Index) && block.IsValid(Blockchain.Difficulty))
            {   
                ResyncLarger("Get this from somewhere", block);
            }
        }
    }

    private Block GetHeadFromNeighbour()
    {
        throw new NotImplementedException();
    }

    private Block GetRemoteBlock(string address, int index)
    {
        throw new NotImplementedException();
    }
    // Step 1  : If remote block is valid and has index = local head index + 1, and previoushash equal to local head
    //           cancel local mining task , add the remote block, and share the new local head.
    // Step 2  : Else if remote block is valid: save current length of blockchain in a variable "LocalChainLength"
    // Step 3  : Check if previousHash on remote equals hash of a block in local chain.
    //           If not, request the previous block from remote and continue to check if previoushash equals a 
    //           hash in local chain. Continue until a match is found or all blocks have been requested,
    //           compare the length of replacing local chain with the remote from the matching hash onwards with "local_chain_length"
    //           if the length of replacing is longer, cancel mining task and do it. Else dont.
    //           Call ShareBlock() with new head. 
    //           If an invalid block is encountered at any time, the process should be aborted.
    private void ResyncLarger(string Address, Block RemoteHead) // figure out better name
    {
        if (!RemoteHead.IsValid(Blockchain.Difficulty))
        {
            return;
        }
        Block localHead = Blockchain.GetHead();
        if ((RemoteHead.Index == localHead.Index + 1) && RemoteHead.PreviousBlockHash == localHead.Hash)
        {
            miningCTSource.Cancel();
            Blockchain.Append(RemoteHead);
            ShareBlock(RemoteHead);
            return;
        }

        int LocalChainLength = Blockchain.Chain.Count();
        Blockchain RemoteChain = new Blockchain(Blockchain.Difficulty);
        RemoteChain.Append(RemoteHead);
        for (int i = RemoteHead.Index; i >= 0; i--)
        {
            Block RemoteBlock = GetRemoteBlock(Address, i);
            RemoteChain.Prepend(RemoteBlock);
            if (!RemoteChain.IsValid())
            {
                return;
            }
            int IndexOfPreviousHash = Blockchain.Chain.FindIndex(b => b.Hash == RemoteBlock.PreviousBlockHash);
            // If no block has hash equal to remote previousblockhash request another block
            if (IndexOfPreviousHash == -1)
            {
                break;
            }
            int LengthOfReplacing = IndexOfPreviousHash + RemoteChain.Chain.Count();
            if (LengthOfReplacing <= LocalChainLength)
            {
                return;
            }
            miningCTSource.Cancel();
            // Delete all blocks from the Index of PreviousBlockHash + 1 and to the end of the chain
            Blockchain.RemoveRange(IndexOfPreviousHash + 1, Blockchain.Chain.Count() - IndexOfPreviousHash + 1);
            Blockchain.Append(RemoteChain.Chain);
            ShareBlock(Blockchain.GetHead());
        }
    }

    // Ask neighbor for entire blockchain
    private void GetBlockchain()
    {
        throw new NotImplementedException();
    }

    // Send newly mined block to all neighbours
    private void ShareBlock(Block block)
    {
        Console.WriteLine($"Broadcasting block {block.Hash}");
        var blockJson = _blockchainSerializer.Serialize(block);
        _networkClient.BroadcastBlock(blockJson);
    }

    // Step 1  : If block has same index or lower than local head, ignore it.
    // Step 2  : Check if block is valid and ignore if not.
    // Step 3  : call ResyncLarger()
    public void ReceiveBlock(Block block) // TODO: Add sender parameter?
    {
        throw new NotImplementedException();
        if (block.Index <= Blockchain.GetHead().Index)
        {
            return;
        }
        if (!block.IsValid(Blockchain.Difficulty))
        {
            return;
        }
        // ResyncLarger(sender, block); //TODO: How to communicate back to sender?
    }

    // ------------------------------------------------------------------------------------------------------------

    public override async Task StartAsync(CancellationToken stoppingToken)
    {
        _networkClient.DiscoverNetwork();
        // ResyncBlockchain();
        // GoOnline();
        _logger.LogInformation("Starting Node");
        await base.StartAsync(stoppingToken);
    }
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        Blockchain.Save();
        await _networkClient.DisconnectFromNetwork();
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
        CancellationToken mineCT = miningCTSource.Token;
        List<Transaction> txs = new List<Transaction>();
        Transaction? transaction;
        for (int i = 0; i < _settings.SizeOfBlocks; i++) // Blocks contain 10 transactions
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
        var addedBlock = Blockchain.MineTransactions(txs, mineCT);
        if (!mineCT.IsCancellationRequested)
        {
            ShareBlock(addedBlock);
        }
        if (mineCT.IsCancellationRequested)
        {
            Console.WriteLine("Cancellation was requested");
            miningCTSource.TryReset();
        }
    }
    public void AddTransaction(Transaction tx)
    {
        _queue.Enqueue(tx);
    }
}