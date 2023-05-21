using Newtonsoft.Json;
using Models;
namespace DCR;
public class Blockchain
{
    private List<Block> _chain;
    private int _difficulty;
    private BlockchainSerializer _chainSerializer;
    private GraphSerializer _graphSerializer;
    public Dictionary<string, (int blockIndex, int transactionIndex)> GraphIdLookupTable;
    public bool DisableGraphIdLookupTable;

    public Blockchain(int difficulty) 
    {
        _difficulty = difficulty;
        _chain = new List<Block>();
        _chainSerializer = new BlockchainSerializer();
        _graphSerializer = new GraphSerializer();
        GraphIdLookupTable = new Dictionary<string, (int, int)>();
    }

    [JsonConstructor]
    private Blockchain(List<Block> chain, int difficulty)
    {
        _chain = chain;
        _difficulty = difficulty;
        _chainSerializer = new BlockchainSerializer();
        _graphSerializer = new GraphSerializer();
        GraphIdLookupTable = new Dictionary<string, (int, int)>();
    }

    public void Initialize(CancellationToken stoppingToken) 
    {
        Block genesis = new Block(new List<Transaction>()) {Index = 0};
        genesis.PreviousBlockHash = "genesis";
        genesis.Mine(_difficulty, stoppingToken);
        Append(genesis);
    }

    public List<Block> Chain 
    {
        get => _chain;
    }

    public int Difficulty 
    {
        get => _difficulty;
    }

    public bool IsValid() 
    {
        if (_chain.Count == 1) { return true; }

        for (int i = 1; i < _chain.Count; i++)
        {
            if (!(PreviousBlockHashValid(i) && _chain[i].IsValid(_difficulty))) 
            { 
                return false; 
            }
        }
        return true;
    }

    private bool PreviousBlockHashValid(int i)
    {
        return _chain[i].PreviousBlockHash == _chain[i - 1].Hash;
    }

    public Block MineTransactions(List<Transaction> tx, CancellationToken stoppingToken) 
    {
        Block block = new Block(tx) {Index = _chain.Count};
        block.PreviousBlockHash = GetHead().Hash;
        block.Mine(Difficulty, stoppingToken);
        if (!stoppingToken.IsCancellationRequested)
        {
            Append(block);
        }
        return block;
    }

    public void RemoveRange(int index, int count)
    {
        if (!DisableGraphIdLookupTable) {
            for (int i = index; i <= index + count; i++) {
                var item = GraphIdLookupTable.SingleOrDefault(x => x.Value.blockIndex == i);
                if (!item.Equals(default(KeyValuePair<string, (int, string)>))) {
                    GraphIdLookupTable.Remove(item.Key);
                }
            }
        }
        _chain.RemoveRange(index, count);
    }
    public void Append(Block block) 
    {
        _chain.Add(block);
        if (!DisableGraphIdLookupTable) {
            UpdateGraphIdLookupTable(block);
        }
    }

    public void Append(List<Block> blocks) 
    {
        foreach (Block block in blocks)
        {
            Append(block);
        }
    }

    public void Prepend(Block block) 
    {
        _chain = _chain.Prepend(block).ToList();
        if (!DisableGraphIdLookupTable) {
            UpdateGraphIdLookupTable(block);
        }
    }

    public Block GetHead()
    {
        return _chain[_chain.Count - 1];
    }

    public Graph? GetGraph(string id) 
    {
        // Use GraphIdLookupTable to directly lookup blockId and transactionId in Blockchain and return graph
        if (!DisableGraphIdLookupTable) {
            if (GraphIdLookupTable.TryGetValue(id, out (int blockIndex, int transactionIndex) idPair)) {
                var graph = _chain[idPair.blockIndex].Transactions[idPair.transactionIndex].Graph;
                return DeepCopyGraph(graph);
            } else {
                return null;
            }
        } else { // Loop through every block and transaction (end to start) and return first occurence of graph
            foreach (Block block in Enumerable.Reverse(_chain))
            {
                foreach (Transaction transaction in Enumerable.Reverse(block.Transactions))
                {
                    if (transaction.Graph.Id == id) 
                    {
                        return DeepCopyGraph(transaction.Graph);
                    }
                }
            }
            return null;
        }
    }

    public Graph DeepCopyGraph(Graph graph)
    {
        return _graphSerializer.Deserialize(_graphSerializer.Serialize(graph));
    }

    // If graph already has an entry in GraphIdLookupTable, update it with new latest block/transaction location. Otherwise, create new entry.
    private void UpdateGraphIdLookupTable(Block block)
    {
        foreach (var (tx, txIndex) in block.Transactions.Select((tx, i) => ( tx, i ))) {
            var idPair = (block.Index, txIndex);
            if (GraphIdLookupTable.ContainsKey(tx.Graph.Id)) {
                GraphIdLookupTable[tx.Graph.Id] = idPair;
            } else {
                GraphIdLookupTable.Add(tx.Graph.Id, idPair);
            }
        }
    }
}