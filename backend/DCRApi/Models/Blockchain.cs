using Newtonsoft.Json;
using Models;
namespace DCR;
public class Blockchain
{
    private List<Block> _chain;
    private int _difficulty;
    private BlockchainSerializer _chainSerializer;
    private GraphSerializer _graphSerializer;

    public Blockchain(int difficulty) 
    {
        _difficulty = difficulty;
        _chain = new List<Block>();
        _chainSerializer = new BlockchainSerializer();
        _graphSerializer = new GraphSerializer();
    }

    [JsonConstructor]
    private Blockchain(List<Block> chain, int difficulty)
    {
        _chain = chain;
        _difficulty = difficulty;
        _chainSerializer = new BlockchainSerializer();
        _graphSerializer = new GraphSerializer();
    }

    public void Initialize(CancellationToken stoppingToken) 
    {
        Block genesis = new Block(new List<Transaction>()) {Index = 0};
        genesis.PreviousBlockHash = "genesis";
        genesis.Mine(_difficulty, stoppingToken);
        _chain.Add(genesis);
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
        _chain.RemoveRange(index, count);
    }
    public void Append(Block block) 
    {
        _chain.Add(block);
    }

    public void Append(List<Block> block) 
    {
        _chain.AddRange(block);
    }

    public void Prepend(Block block) 
    {
        _chain = _chain.Prepend(block).ToList();
    }

    public Block GetHead()
    {
        return _chain[_chain.Count - 1];
    }

    public Graph? GetGraph(string id) 
    {
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

    public Graph DeepCopyGraph(Graph graph)
    {
        return _graphSerializer.Deserialize(_graphSerializer.Serialize(graph));
    }
}