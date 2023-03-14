using Newtonsoft.Json;
namespace DCR;
public class BlockChain
{
    private List<Block> _chain;
    private int _difficulty;

    public BlockChain(int difficulty) 
    {
        _difficulty = difficulty;
        _chain = new List<Block>();
        Initialize();
    }

    [JsonConstructor]
    private BlockChain(List<Block> chain, int difficulty)
    {
        _chain = chain;
        _difficulty = difficulty;
    }

    private void Initialize() 
    {
        Block genesis = new Block(new List<Transaction>());
        genesis.PreviousBlockHash = "genesis";
        genesis.Mine(_difficulty);
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

    public void AddBlock(Block block) 
    {
        block.PreviousBlockHash = GetHead().Hash;
        block.Mine(Difficulty);
        _chain.Add(block);  
    }

    public Block GetHead()
    {
        return _chain[_chain.Count - 1];
    }

    public string GetGraph(string id) 
    {
        foreach (Block block in _chain)
        {
            foreach (Transaction transaction in block.Transactions)
            {
                if (transaction.Graph == id) 
                {
                    return transaction.Graph;
                }
            }
        }
        return "Could not find id";
    }
}