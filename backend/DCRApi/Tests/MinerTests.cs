using NUnit.Framework;
using System.Diagnostics;
using DCR;
using Newtonsoft.Json;

namespace Tests;

public class MinerTests
{
    private GraphSerializer _graphSerializer;
    private BlockchainSerializer _blockchainSerializer;
    private Settings _settings;
    private Miner _miner;

    [SetUp]
    public void Setup()
    {
        _graphSerializer = new GraphSerializer();
        _blockchainSerializer = new BlockchainSerializer();
        var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<Miner>();
        var networkClient = new NetworkClient("localhost", 4300);
        _settings = new Settings() {
            TimeToSleep = 0,
            SizeOfBlocks = int.MaxValue,
            NumberNeighbours = 1,
            Difficulty = 0,
            NumEvalTransactions = 10000
        };
        _miner = new Miner(logger, networkClient, _settings);
    }

    [Test]
    public void Evaluate_DequeueTransactions()
    {
        // Enqueue Create transactions
        EnqueueTransactions(_settings.NumEvalTransactions);

        // Evaluated transactions dequeue/validation
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        stopwatch.Start();
        cancellationTokenSource.CancelAfter(4000);
        var actualTxs = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();

        Console.WriteLine($"Elapsed validation time: {stopwatch.Elapsed.TotalMilliseconds} ms");
        Console.WriteLine($"Number of validated transactions: {actualTxs.Count()} / {_settings.NumEvalTransactions}");
        Assert.AreEqual(_settings.NumEvalTransactions, actualTxs.Count());
    }
    [Test]
    public void Evaluate_DequeueTransactions_FilledBlockchain()
    {
        var graphJsonWithId = """{"Id":"evalgraph","Activities":[{"Title":"Select papers","Pending":true,"Included":true,"Executed":false,"Enabled":true},{"Title":"Write introduction","Pending":true,"Included":true,"Executed":false,"Enabled":false},{"Title":"Write abstract","Pending":true,"Included":true,"Executed":false,"Enabled":false},{"Title":"Write conclusion","Pending":true,"Included":true,"Executed":false,"Enabled":true}],"Relations":[{"Type":2,"Source":"Select papers","Target":"Select papers"},{"Type":0,"Source":"Select papers","Target":"Write introduction"},{"Type":0,"Source":"Select papers","Target":"Write abstract"},{"Type":2,"Source":"Select papers","Target":"Write conclusion"},{"Type":1,"Source":"Write introduction","Target":"Write abstract"},{"Type":1,"Source":"Write conclusion","Target":"Write abstract"}],"Accepting":false}"""; 

        // Enqueue Create transactions
        int sizeOfBlockchain = 1;
        // Evaluated transactions dequeue/validation
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        EnqueueTransactionsWithExecutions(graphJsonWithId, _settings.NumEvalTransactions, sizeOfBlockchain);
        stopwatch.Start();
        cancellationTokenSource.CancelAfter(4000);
        var actualTxs = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();

        Console.WriteLine($"Elapsed validation time: {stopwatch.Elapsed.TotalMilliseconds} ms");
        Console.WriteLine($"Number of validated transactions: {actualTxs.Count()} / {_settings.NumEvalTransactions}");
        Assert.AreEqual(_settings.NumEvalTransactions, actualTxs.Count());
    }

    [Test]
    public void TestValidation()
    {
        var graphJsonWithId = """{"Id":"evalgraph","Activities":[{"Title":"Select papers","Pending":true,"Included":true,"Executed":false,"Enabled":true},{"Title":"Write introduction","Pending":true,"Included":true,"Executed":false,"Enabled":false},{"Title":"Write abstract","Pending":true,"Included":true,"Executed":false,"Enabled":false},{"Title":"Write conclusion","Pending":true,"Included":true,"Executed":false,"Enabled":true}],"Relations":[{"Type":2,"Source":"Select papers","Target":"Select papers"},{"Type":0,"Source":"Select papers","Target":"Write introduction"},{"Type":0,"Source":"Select papers","Target":"Write abstract"},{"Type":2,"Source":"Select papers","Target":"Write conclusion"},{"Type":1,"Source":"Write introduction","Target":"Write abstract"},{"Type":1,"Source":"Write conclusion","Target":"Write abstract"}],"Accepting":false}"""; 

        // Enqueue Create transactions
        int sizeOfBlockchain = 1;
        // Evaluated transactions dequeue/validation
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        CreateTransactionsWithInvalidId(graphJsonWithId, _settings.NumEvalTransactions, sizeOfBlockchain);
        var noTxs = _miner.DequeueTransactions(cancellationToken);
        Assert.AreEqual(0, noTxs.Count());
        EnqueueTransactionsWithExecutions(graphJsonWithId, 1000, 1);
        var txs = _miner.DequeueTransactions(cancellationToken);
        Assert.AreEqual(1000, txs.Count());
    }

    private Models.Graph CreateGraph()
    {
        List<Models.Activity> activities = new List<Models.Activity>(){
            new Models.Activity("Select papers", true, true, false), 
            new Models.Activity("Write introduction", true, true, false), 
            new Models.Activity("Write abstract", true, true, false), 
            new Models.Activity("Write conclusion", true, true, false)};
        List<Models.Relation> relations = new List<Models.Relation>(){
            new Models.Relation(Models.RelationType.INCLUSION, "Select papers", "Select papers"),
            new Models.Relation(Models.RelationType.CONDITION, "Select papers", "Write introduction"),
            new Models.Relation(Models.RelationType.CONDITION, "Select papers", "Write abstract"),
            new Models.Relation(Models.RelationType.INCLUSION, "Select papers", "Write conclusion"),
            new Models.Relation(Models.RelationType.RESPONSE, "Write introduction", "Write abstract"),
            new Models.Relation(Models.RelationType.RESPONSE, "Write conclusion", "Write abstract"),
        };
        return new Models.Graph(activities, relations);
    }
    private void EnqueueTransactions(int numTransactions) {
        for (int i = 0; i < numTransactions; i++) {
            Models.Graph graph = CreateGraph();
            var tx = new Transaction("eval", DCR.Action.Create, "", graph);
            _miner.HandleTransaction(tx);
        }
    }
    private void EnqueueTransactionsWithExecutions(string graphJsonWithId, int numTransactions, int blockchainSize) {
        var graph = _graphSerializer.Deserialize(graphJsonWithId);
        var createTx = new Transaction("eval", DCR.Action.Create, "", graph);
        _miner.HandleTransaction(createTx);
        HandleOtherGraphs(blockchainSize);
        for (int i = 0; i < numTransactions; i++) {
            var graphToUpdate = _graphSerializer.Deserialize(graphJsonWithId);
            graphToUpdate.Execute("Select papers");
            var tx = new Transaction("1", DCR.Action.Update, "Select papers", graphToUpdate);
            _miner.HandleTransaction(tx);
        }
    }
    private void CreateTransactionsWithInvalidId(string graphJsonWithId, int numTransactions, int blockchainSize) {
        var graph = _graphSerializer.Deserialize(graphJsonWithId);
        var createTx = new Transaction("eval", DCR.Action.Create, "", graph);
        _miner.HandleTransaction(createTx);
        HandleOtherGraphs(blockchainSize);
        for (int i = 0; i < numTransactions; i++) {
            var graphJsonWithWrongId = """{"Id":"Foo","Activities":[{"Title":"Select papers","Pending":true,"Included":true,"Executed":false,"Enabled":true},{"Title":"Write introduction","Pending":true,"Included":true,"Executed":false,"Enabled":false},{"Title":"Write abstract","Pending":true,"Included":true,"Executed":false,"Enabled":false},{"Title":"Write conclusion","Pending":true,"Included":true,"Executed":false,"Enabled":true}],"Relations":[{"Type":2,"Source":"Select papers","Target":"Select papers"},{"Type":0,"Source":"Select papers","Target":"Write introduction"},{"Type":0,"Source":"Select papers","Target":"Write abstract"},{"Type":2,"Source":"Select papers","Target":"Write conclusion"},{"Type":1,"Source":"Write introduction","Target":"Write abstract"},{"Type":1,"Source":"Write conclusion","Target":"Write abstract"}],"Accepting":false}"""; 
            var graphToUpdate = _graphSerializer.Deserialize(graphJsonWithWrongId);
            graphToUpdate.Execute("Select papers");
            var tx = new Transaction("1", DCR.Action.Update, "Select papers", graphToUpdate);
            _miner.HandleTransaction(tx);
        }
    }

    private void HandleOtherGraphs(int blockchainSize)
    {
        for (int i = 1; i < blockchainSize; i++)
        {
            var graph = CreateGraph();
            var createTx = new Transaction("eval", DCR.Action.Create, "", graph);
            _miner.HandleTransaction(createTx);
        }
        _miner.Mine();
    }
}