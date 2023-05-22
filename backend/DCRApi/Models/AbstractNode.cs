namespace DCR;

using Newtonsoft.Json;
using Models;

public abstract class AbstractNode
{
    protected int Id = new Random().Next();
    private string _blockchainFilename { get; }
    public List<Transaction> HandledTransactions {get; init;}
    public Blockchain Blockchain {get; init;}
    protected readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    private readonly GraphSerializer _graphSerializer = new GraphSerializer();
    public NetworkClient NetworkClient {get; init;}
    protected Settings _settings;
    // miningCTSource is present in all nodes, to allow for the same implementation in resyncing blockchain
    // even if it is not used in other node types than miner.
    public CancellationTokenSource miningCTSource = new CancellationTokenSource();
    public abstract void HandleTransaction(Transaction tx);


    public AbstractNode(NetworkClient networkClient, Settings settings)
    {
        NetworkClient = networkClient;
        HandledTransactions = new List<Transaction>();
        _blockchainFilename = $"blockchain{NetworkClient.ClientNode.Port}.json";
        _settings = settings;

        if (System.IO.File.Exists(_blockchainFilename)) {
            // Load local blockchain and sync with neighbors
            Console.WriteLine("Loading saved blockchain");
            var blockchainJson = System.IO.File.ReadAllText(_blockchainFilename);
            Blockchain = _blockchainSerializer.Deserialize(blockchainJson);
            ResyncBlockchain(_settings.NumberNeighbours);
        } else {
            var neighborBlockchain = GetRandomBlockchain(_settings.NumberNeighbours).Result;
            if (neighborBlockchain is not null) {
                // Full sync with random neighbors
                Console.WriteLine("Synced with a neighbor blockchain");
                Blockchain = neighborBlockchain;
            } else {
                // Create new blockchain
                Console.WriteLine("Could not sync with neighbor blockchains, creating new");
                Blockchain = new Blockchain(_settings.Difficulty);
                Blockchain.Initialize(miningCTSource.Token);
            }
        }
        Save();
    }

    private async Task<Blockchain?> GetRandomBlockchain(int numberNeighbors) {
        var randomNeighbors = NetworkClient.GetRandomNeighbors(numberNeighbors);
        foreach (var neighbor in randomNeighbors)
        {
            var neighborBlockchain = await NetworkClient.GetBlockchain(neighbor);
            if (neighborBlockchain is not null) {
                return neighborBlockchain;
            }
        }
        return null;
    }

    private async void ResyncBlockchain(int numberNeighbors)
    {
        var randomNeighbors = NetworkClient.GetRandomNeighbors(numberNeighbors);
        foreach (var neighbor in randomNeighbors) {
            var remoteHead = await NetworkClient.GetHead(neighbor);
            if (remoteHead is null) {
                continue;
            }

            var localHead = Blockchain.GetHead();
            if (remoteHead.Index == localHead.Index) // Client blockchain is up to date
            {
                return;
            }

            if ((remoteHead.Index > localHead.Index) && remoteHead.IsValid(Blockchain.Difficulty)) // Client blockchain is behind
            {
                ResyncBlockchainWithNode(neighbor, remoteHead);
            }
        }     
    }

    // TODO: Fix possible race condition.
    // First check is done an instant before a new block is added.
    // new block is added before mining task is cancelled
    private async void ResyncBlockchainWithNode(NetworkNode node, Block remoteHead)
    {
        var localHead = Blockchain.GetHead();
        if ((remoteHead.Index == localHead.Index + 1) && remoteHead.PreviousBlockHash == localHead.Hash) // RemoteHead should be client's next block
        {
            miningCTSource.Cancel();
            Blockchain.Append(remoteHead);
            ShareBlock(remoteHead);
            Save();
            return;
        }

        int localChainLength = Blockchain.Chain.Count();
        var reconstructedChain = new Blockchain(Blockchain.Difficulty);
        reconstructedChain.Append(remoteHead);
        for (int i = remoteHead.Index-1; i >= 0; i--)
        {
            var remoteBlock = await NetworkClient.GetBlock(node, i);
            if (remoteBlock is null || !reconstructedChain.IsValid())
            {
                Console.WriteLine($"Failed to perform resync with neighbor {node.URL}");
                return;
            }
            reconstructedChain.Prepend(remoteBlock);
            var indexOfPreviousHash = Blockchain.Chain.FindIndex(b => b.Hash == remoteBlock.PreviousBlockHash);
            // If no block has hash equal to remote previousblockhash request another block
            if (indexOfPreviousHash == -1)
            {
                continue;
            }
            var lengthOfReplacing = indexOfPreviousHash + reconstructedChain.Chain.Count();
            if (lengthOfReplacing <= localChainLength)
            {
                return;
            }
            miningCTSource.Cancel();
            // Delete all blocks from the Index of PreviousBlockHash + 1 and to the end of the chain
            Blockchain.RemoveRange(indexOfPreviousHash + 1, Blockchain.Chain.Count() - indexOfPreviousHash - 1);
            Blockchain.Append(reconstructedChain.Chain);
            ShareBlock(Blockchain.GetHead());
            Save();
        }
    }

    protected void ShareBlock(Block block)
    {
        var blockJson = JsonConvert.SerializeObject(block);
        if (block.Transactions.Any()) {
            Console.WriteLine($"Broadcasting block {blockJson}");
        }
        NetworkClient.BroadcastBlock(block);
    }

    public virtual bool ReceiveBlock(NetworkNode sender, Block receivedBlock)
    {
        lock(Blockchain)
        {
            if (receivedBlock.Index <= Blockchain.GetHead().Index || !receivedBlock.IsValid(Blockchain.Difficulty))
            {
                return false;
            }
            ResyncBlockchainWithNode(sender, receivedBlock);
            return true;
        }
    }
    private bool GraphIdExists(Transaction tx)
    {
        return Blockchain.GetGraph(tx.Graph.Id) is not null;
    }
    protected bool IsValidTransaction(Transaction tx, List<Transaction> txs) {
        if (tx.Action == Action.Create) {
            return (tx.Graph.Id != "") && (Blockchain.DisableGraphIdLookupTable ? !GraphIdExists(tx) : !Blockchain.GraphIdLookupTable.ContainsKey(tx.Graph.Id));
        } else {
            var transaction = _settings.IsEval ? null : txs.FindLast(t => t.Graph.Id == tx.Graph.Id);
            var oldGraph = transaction is not null ? transaction.Graph : Blockchain.GetGraph(tx.Graph.Id);
            if (oldGraph is not null) { 
                var expectedUpdatedGraph = Blockchain.DeepCopyGraph(oldGraph);
                return expectedUpdatedGraph.Execute(tx.EntityTitle) && tx.Graph.EqualsGraph(expectedUpdatedGraph);
            }
            return false;
        }
    }

    protected void Save()
    {
        var blockchainJson = _blockchainSerializer.Serialize(Blockchain);
        using (StreamWriter sw = System.IO.File.CreateText(_blockchainFilename))
        {
            sw.Write(blockchainJson);
        }
    }
}