namespace DCR;
public class Block
{
    private string _previousBlockHash;
    private DateTime _timestamp;
    private int _nonce;
    private List<Transaction> _transactions;

    public Block(string previousBlockHash, List<Transaction> transactions) 
    {
        _previousBlockHash = previousBlockHash;
        _timestamp = DateTime.Now;
        _nonce = 0;
        _transactions = transactions;
    }
    public string PreviousBlockHash 
    {
        get => _previousBlockHash;
    }

    public DateTime Timestamp 
    {
        get => _timestamp;
    }

    public int Nonce 
    {
       get => _nonce;
    }

    public List<Transaction> Transactions 
    {
        get => _transactions;
    }

    public string GetHash() 
    {
        throw new NotImplementedException();
    }
    
    public void Mine()
    {
        throw new NotImplementedException();
    }
}