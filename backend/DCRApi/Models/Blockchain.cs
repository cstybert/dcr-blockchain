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
        Save();
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
            if (!(PreviousBlockHashValid(i) && CurrentBlockValid(i))) 
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

    private bool CurrentBlockValid(int i)
    {
        string leadingzeroes = new string('0', _difficulty);
        // Check leading zeroes and hash are correct
        return  (_chain[i].Hash.Substring(0, _difficulty) == leadingzeroes) 
             && (_chain[i].Hash == _chain[i].GetHash());
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
        Save();
    }

    public void Append(List<Block> block) 
    {
        _chain.AddRange(block);
        Save();
    }

    public void Prepend(Block block) 
    {
        _chain.Prepend(block);
        Save();
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
                if (transaction.Graph.ID == id) 
                {
                    return transaction.Graph;
                }
            }
        }
        return null;
    }

    public void Save()
    {
        var blockchainJson = _chainSerializer.Serialize(this);
        using (StreamWriter sw = System.IO.File.CreateText("blockchain.json"))
        {
            sw.Write(blockchainJson);
        }
    }
}