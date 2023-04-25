using System.Diagnostics;
using DCR;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;

public class Evaluation{
    private NetworkClient _networkClient = new NetworkClient("localhost", 4300);
    private ILogger<Miner> _loggerMiner;
    private Miner _miner;
    private int HandledTransactions = 0;

    public Evaluation()
    {
        _loggerMiner = LoggerFactory
            .Create(logging => logging.AddConsole())
            .CreateLogger<Miner>();
        _miner = new Miner(_loggerMiner, _networkClient);
    }
    public async void Eval()
    {
        int i = 0;
        while (i < Settings.NumEvalTransactions)
        {
            if (i % 1000 == 0 ) Console.WriteLine($"Added {i} transactions");
            string graphstring = """{"Actor":"Foo","Activities":[{"title":"Select papers","pending":true,"included":true,"executed":false,"enabled":true},{"title":"Write introduction","pending":true,"included":true,"executed":false,"enabled":false},{"title":"Write abstract","pending":true,"included":true,"executed":false,"enabled":false},{"title":"Write conclusion","pending":true,"included":true,"executed":false,"enabled":false}],"Relations":[{"source":"Select papers","type":2,"target":"Select papers"},{"source":"Select papers","type":0,"target":"Write introduction"},{"source":"Select papers","type":0,"target":"Write abstract"},{"source":"Select papers","type":0,"target":"Write conclusion"},{"source":"Write introduction","type":1,"target":"Write abstract"},{"source":"Write conclusion","type":1,"target":"Write abstract"}]}""";
            Graph graph = JsonConvert.DeserializeObject<Graph>(graphstring)!;
            Transaction tx = new Transaction("eval", DCR.Action.Create, "", graph);
            _miner.HandleTransaction(tx);
            i++;
        } 
        Task task = new Task(new System.Action(_miner.Mine));
        task.Start();
        Thread.Sleep(5000);
        task.Dispose();

        string blockchainFilename = $"blockchain4300.json";
        var blockchainJson = System.IO.File.ReadAllText(blockchainFilename);
        Blockchain blockchain = JsonConvert.DeserializeObject<Blockchain>(blockchainJson)!;
        Console.WriteLine($"Amount of transactions in block: {blockchain.GetHead().Transactions.Count}");
    }
}