using NUnit.Framework;
using System.Diagnostics;
using DCR;
using Models;

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
            NumEvalTransactions = 5000
        };
        _miner = new Miner(logger, networkClient, _settings);
    }

    [Test]
    public void Evaluate_DequeueAndValidateTransactions_Create()
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var graph = CreateMeetingGraph();

        // Enqueue transactions
        EnqueueCreateTransactions(graph, _settings.NumEvalTransactions);

        // Measure dequeue+validation time
        stopwatch.Start();
        cancellationTokenSource.CancelAfter(2400);
        var actualTxs = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();

        Console.WriteLine($"Elapsed validation time: {stopwatch.Elapsed.TotalMilliseconds} ms");
        Console.WriteLine($"Number of validated transactions: {actualTxs.Count()} / {_settings.NumEvalTransactions}");
        Assert.AreEqual(_settings.NumEvalTransactions, actualTxs.Count());
    }
    [Test]
    public void Evaluate_DequeueAndValidateTransactions_Execute()
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var graph = CreatePaperGraph("eval");

        // Enqueue transactions
        EnqueueCreateTransactions(graph, 10000);
        EnqueueExecuteTransactions(graph, "Select papers", _settings.NumEvalTransactions);

        // Measure dequeue+validation time
        stopwatch.Start();
        cancellationTokenSource.CancelAfter(2400);
        var actualTxs = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();

        Console.WriteLine($"Elapsed validation time: {stopwatch.Elapsed.TotalMilliseconds} ms");
        Console.WriteLine($"Number of validated transactions: {actualTxs.Count()} / {_settings.NumEvalTransactions}");
        Assert.AreEqual(_settings.NumEvalTransactions, actualTxs.Count());
    }

    [Test]
    public void Test_TransactionValidation_Valid()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var graphFoo = CreatePaperGraph("foo");

        EnqueueCreateTransactions(graphFoo, 1);
        EnqueueExecuteTransactions(graphFoo, "Select papers", 1000);
        var validTxs = _miner.DequeueTransactions(cancellationToken);

        Assert.AreEqual(1001, validTxs.Count());
    }

    [Test]
    public void Test_TransactionValidation_Invalid()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var graphFoo = CreatePaperGraph("foo");
        var graphBar = CreatePaperGraph("bar");

        EnqueueCreateTransactions(graphFoo, 1);
        EnqueueExecuteTransactions(graphBar, "Select papers", 1000);

        var validTxs = _miner.DequeueTransactions(cancellationToken);
        Assert.AreEqual(1, validTxs.Count());
    }

    [Test]
    public void Test_TransactionValidation_GraphIdLookupTable()
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var graphFoo = CreatePaperGraph("foo");

        // Set up blockchain
        EnqueueCreateTransactions(graphFoo, 1);
        for (int i = 0; i < 10000; i++) {
            var fillerGraph = CreatePaperGraph(Guid.NewGuid().ToString());
            EnqueueCreateTransactionsWithId(i.ToString(), fillerGraph, 1);
        }
        var validTxs = _miner.DequeueTransactions(cancellationToken);
        foreach (Transaction tx in validTxs) {
            MockMine(new List<Transaction>{ tx });
        }


        // Measure ms of validating execution with GraphIdLookupTable
        EnqueueExecuteTransactions(graphFoo, "Select papers", 1);
        
        stopwatch.Start();
        _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();

        var msWith = stopwatch.Elapsed.TotalMilliseconds;

        // Measure ms of execution without GraphIdLookupTable
        EnqueueExecuteTransactions(graphFoo, "Select papers", 1);
        _miner.Blockchain.DisableGraphIdLookupTable = true;
        stopwatch.Reset();

        stopwatch.Start();
        _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();

        var msWithout = stopwatch.Elapsed.TotalMilliseconds;

        Console.WriteLine($"{msWith} ms vs {msWithout} ms");
        Assert.IsTrue(msWith < msWithout);
    }

    private void EnqueueCreateTransactionsWithId(string id, Graph graph, int numTransactions) {
        for (int i = 0; i < numTransactions; i++) {
            var tx = new Transaction(id, DCR.Action.Create, "", graph);
            _miner.HandleTransaction(tx);
        }
    }

    private void EnqueueCreateTransactions(Graph graph, int numTransactions) {
        EnqueueCreateTransactionsWithId("eval", graph, numTransactions);
    }

    private void EnqueueExecuteTransactions(Graph graph, string executeActivity, int numTransactions) {
        for (int i = 0; i < numTransactions; i++) {
            var graphToUpdate = _miner.Blockchain.DeepCopyGraph(graph);
            graphToUpdate.Execute(executeActivity);
            var tx = new Transaction("1", DCR.Action.Update, executeActivity, graphToUpdate);
            _miner.HandleTransaction(tx);
        }
    }

    private void MockMine(List<Transaction> txs) {
        var block = new Block(txs) {Index = _miner.Blockchain.Chain.Count};
        _miner.Blockchain.Append(block);
    }

    private Graph CreatePaperGraph(string id)
    {
        var graphJson = """{"Activities":[{"Title":"Select papers","Pending":true,"Included":true,"Executed":false,"Enabled":true},{"Title":"Write introduction","Pending":true,"Included":true,"Executed":false,"Enabled":false},{"Title":"Write abstract","Pending":true,"Included":true,"Executed":false,"Enabled":false},{"Title":"Write conclusion","Pending":true,"Included":true,"Executed":false,"Enabled":true}],"Relations":[{"Type":2,"Source":"Select papers","Target":"Select papers"},{"Type":0,"Source":"Select papers","Target":"Write introduction"},{"Type":0,"Source":"Select papers","Target":"Write abstract"},{"Type":2,"Source":"Select papers","Target":"Write conclusion"},{"Type":1,"Source":"Write introduction","Target":"Write abstract"},{"Type":1,"Source":"Write conclusion","Target":"Write abstract"}],"Accepting":false}""";
        var graph = _graphSerializer.Deserialize(graphJson);
        graph.Id = id;
        return graph;
    }

    private Graph CreateMeetingGraph()
    {
        var proposeDU = new Models.Activity("Propose - DU");
        var proposeDE = new Models.Activity("Propose - DE");
        var acceptDU = new Models.Activity("Accept - DU");
        var acceptDE = new Models.Activity("Accept - DE");
        var holdMeeting = new Models.Activity("Hold Meeting", true);
        var activities = new List<Models.Activity> {proposeDU, proposeDE, acceptDU, acceptDE, holdMeeting};

        var rel1 = new Relation(RelationType.CONDITION, proposeDU, proposeDE);
        var rel2 = new Relation(RelationType.RESPONSE, proposeDU, acceptDE);
        var rel3 = new Relation(RelationType.INCLUSION, proposeDU, acceptDE);
        var rel4 = new Relation(RelationType.RESPONSE, proposeDE, acceptDU);
        var rel5 = new Relation(RelationType.INCLUSION, proposeDE, acceptDU);
        var rel6 = new Relation(RelationType.EXCLUSION, acceptDU, acceptDU);
        var rel7 = new Relation(RelationType.EXCLUSION, acceptDU, acceptDE);
        var rel8 = new Relation(RelationType.EXCLUSION, acceptDE, acceptDE);
        var rel9 = new Relation(RelationType.EXCLUSION, acceptDE, acceptDU);
        var rel10 = new Relation(RelationType.CONDITION, acceptDU, holdMeeting);
        var rel11 = new Relation(RelationType.CONDITION, acceptDE, holdMeeting);
        var relations = new List<Relation> {rel1, rel2, rel3, rel4, rel5, rel6, rel7, rel8, rel9, rel10, rel11};

        return new Graph(activities, relations);
    }
}