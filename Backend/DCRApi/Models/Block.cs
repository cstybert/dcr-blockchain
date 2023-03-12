using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace DCR;
public class Block
{
    public string? PreviousBlockHash {get; set;}
    private DateTime _timestamp;
    public string Hash {get; private set;}
    public int Nonce {get; private set;}
    private List<Transaction> _transactions;

    public Block(List<Transaction> transactions) 
    {
        _timestamp = DateTime.Now;
        Nonce = 0;
        _transactions = transactions;
        Hash = "";
    }

    public DateTime Timestamp 
    {
        get => _timestamp;
    }

    public List<Transaction> Transactions 
    {
        get => _transactions;
    }

    public string GetHash() 
    {
        string jsonTx = JsonConvert.SerializeObject(_transactions);
        // Hash to be calculated for all fields except Hash
        string inputstring = $"{PreviousBlockHash}{_timestamp}{Nonce}{jsonTx}";

        byte[] inputbytes = Encoding.ASCII.GetBytes(inputstring);
        byte[] hash = SHA256.HashData(inputbytes);
        return Convert.ToBase64String(hash);
    }
    
    public void Mine(int difficulty)
    {
        Hash = GetHash();
        string leadingzeroes = new string('0', difficulty);
        while (Hash.Substring(0, difficulty) != leadingzeroes) 
        {
            Nonce++;
            Hash = GetHash();
        }
    }
}