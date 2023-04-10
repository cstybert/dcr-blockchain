using System.Reflection.Emit;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace DCR;
public class Block
{
    public string? PreviousBlockHash {get; set;}
    private DateTime _timestamp;
    public string Hash {get; set;}
    public int Nonce {get; set;}
    private List<Transaction> _transactions;
    public int Index {get; init;}

    public Block(List<Transaction> transactions) 
    {
        _timestamp = DateTime.Now;
        Nonce = 0;
        _transactions = transactions;
        Hash = "";
    }

    [JsonConstructor]
    private Block(string pvhash, DateTime timestamp, string hash, int nonce,  List<Transaction> transactions) 
    {
        PreviousBlockHash = pvhash;
        _timestamp = Timestamp;
        Hash = hash;
        Nonce = nonce;
        _transactions = transactions;
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
        string inputstring = $"{PreviousBlockHash}{Timestamp}{Nonce}{jsonTx}";

        byte[] inputbytes = Encoding.ASCII.GetBytes(inputstring);
        byte[] hash = SHA256.HashData(inputbytes);
        return Convert.ToBase64String(hash);
    }
    
    public void Mine(int difficulty, CancellationToken stoppingToken)
    {
        Hash = GetHash();
        string leadingzeroes = new string('0', difficulty);
        while ((Hash.Substring(0, difficulty) != leadingzeroes)) 
        {
            if(stoppingToken.IsCancellationRequested)
            {
                return;
            }
            Nonce++;
            Hash = GetHash();
        }
    }

    public bool IsValid(int Difficulty)
    {
        Console.WriteLine("checking validity of block");
        string leadingzeroes = new string('0', Difficulty);
        // Check leading zeroes and hash are correct
        bool leadingzeroestruth = Hash.Substring(0, Difficulty) == leadingzeroes;
        Console.WriteLine($"Did have enough leading zeroes {leadingzeroestruth}");
        Console.WriteLine($"The hash is {Hash}");
        Console.WriteLine($"GetHash is  {GetHash()}");
        Console.WriteLine("The block is : " + JsonConvert.SerializeObject(this));
        return  (Hash.Substring(0, Difficulty) == leadingzeroes) 
             && (Hash == GetHash());
    }
}