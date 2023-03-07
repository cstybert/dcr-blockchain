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

    private void Initialize() 
    {
        Block genesis = new Block("0", new List<Transaction>());
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
        throw new NotImplementedException();
    }

    public void AddBlock(Block block) 
    {
        throw new NotImplementedException();
    }
}