using Newtonsoft.Json;
using Models;
namespace DCR;
public class Blockchain
{
    private List<Block> _chain;
    private int _difficulty;
    private BlockchainSerializer _chainSerializer;
    private GraphSerializer _graphSerializer;
    public Dictionary<string, (int blockId, string transactionId)> GraphIdLookupTable;

    public Blockchain(int difficulty) 
    {
        _difficulty = difficulty;
        _chain = new List<Block>();
        _chainSerializer = new BlockchainSerializer();
        _graphSerializer = new GraphSerializer();
        GraphIdLookupTable = new Dictionary<string, (int, string)>();
    }

    [JsonConstructor]
    private Blockchain(List<Block> chain, int difficulty)
    {
        _chain = chain;
        _difficulty = difficulty;
        _chainSerializer = new BlockchainSerializer();
        _graphSerializer = new GraphSerializer();
        GraphIdLookupTable = new Dictionary<string, (int, string)>();
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
        for (int i = index; i <= index + count; i++) {
            var item = GraphIdLookupTable.SingleOrDefault(x => x.Value.blockId == i);
            if (!item.Equals(default(KeyValuePair<string, (int, string)>))) {
                GraphIdLookupTable.Remove(item.Key);
            }
        }
        _chain.RemoveRange(index, count);
    }
    public void Append(Block block) 
    {
        _chain.Add(block);
        UpdateGraphIdLookupTable(block);
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
        UpdateGraphIdLookupTable(block);
    }

    public Block GetHead()
    {
        return _chain[_chain.Count - 1];
    }

    // Directly lookup blockId and transactionId in Blockchain, return graph
    public Graph? GetGraph(string id) 
    {
        if (GraphIdLookupTable.TryGetValue(id, out (int blockId, string transactionId) idPair)) {
            var graph = _chain[idPair.blockId].Transactions.Single(t => t.Id == idPair.transactionId)?.Graph;
            return DeepCopyGraph(graph);
        } else {
            return null;
        }
    }

    public Graph DeepCopyGraph(Graph graph)
    {
        return _graphSerializer.Deserialize(_graphSerializer.Serialize(graph));
    }

    private void UpdateGraphIdLookupTable(Block block)
    {
        foreach (Transaction tx in block.Transactions) {
            var idPair = (block.Index, tx.Id);
            if (GraphIdLookupTable.ContainsKey(tx.Graph.Id)) {
                GraphIdLookupTable[tx.Graph.Id] = idPair;
            } else {
                GraphIdLookupTable.Add(tx.Graph.Id, idPair);
            }
        }
    }
}