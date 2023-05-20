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
    private int _sizeOfBlockchain;
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
            NumEvalTransactions = 50000,
            IsEval = true
        };
        _sizeOfBlockchain = 20000;
        _miner = new Miner(logger, networkClient, _settings);
    }

    [Test]
    public void Test_TransactionValidation_Valid()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var graphFoo = CreatePaperGraph("foo");

        EnqueueCreateTransactions(graphFoo, 1);
        var createTx = _miner.DequeueTransactions(cancellationToken);
        MockMine(createTx);
        EnqueueExecuteTransactions(graphFoo, "Select papers", 1000);
        var validTxs = _miner.DequeueTransactions(cancellationToken);

        Assert.AreEqual(1000, validTxs.Count());
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
    public void Test_TransactionValidation_GraphIdLookupTable_Create()
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        // Set up blockchain
        for (int i = 0; i < _sizeOfBlockchain; i++) {
            var fillerGraph = CreatePaperGraph(Guid.NewGuid().ToString());
            EnqueueCreateTransactionsWithId(i.ToString(), fillerGraph, 1);
        }
        var validTxs = _miner.DequeueTransactions(cancellationToken);
        MockMine(validTxs);
        var graph = CreateMeetingGraph();
        // Measure ms of validating create with GraphIdLookupTable
        EnqueueCreateTransactions(graph, _settings.NumEvalTransactions);
        
        stopwatch.Start();
        var validTxsBefore = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();

        var msWith = stopwatch.Elapsed.TotalMilliseconds;

        // Measure ms of execution without GraphIdLookupTable
        EnqueueCreateTransactions(graph, _settings.NumEvalTransactions);
        _miner.Blockchain.DisableGraphIdLookupTable = true;
        stopwatch.Reset();

        stopwatch.Start();
        var validTxsAfter = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();

        var msWithout = stopwatch.Elapsed.TotalMilliseconds;
        var validationTime = 2400;
        Console.WriteLine("Total Time");
        Console.WriteLine($"With : {msWith} ms  --  Without : {msWithout} ms");
        Console.WriteLine("Time per transaction");
        Console.WriteLine($"With : {msWith/_settings.NumEvalTransactions} ms  --  Without : {msWithout/_settings.NumEvalTransactions} ms");
        Console.WriteLine("Theoretical Block Size");
        Console.WriteLine($"With : {validationTime/(msWith/_settings.NumEvalTransactions)} --  Without : {validationTime/(msWithout/_settings.NumEvalTransactions)}");
        Assert.IsTrue(msWith < msWithout);
        Assert.IsTrue(validTxsBefore.Count == _settings.NumEvalTransactions);
        Assert.IsTrue(validTxsAfter.Count == _settings.NumEvalTransactions);
    }

    [Test]
    public void Test_TransactionValidation_GraphIdLookupTable_Execute()
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var graphFoo = CreatePaperGraph("foo");

        // Set up blockchain
        EnqueueCreateTransactions(graphFoo, 1);
        for (int i = 0; i < _sizeOfBlockchain; i++) {
            var fillerGraph = CreatePaperGraph(Guid.NewGuid().ToString());
            EnqueueCreateTransactionsWithId(i.ToString(), fillerGraph, 1);
        }
        var validTxs = _miner.DequeueTransactions(cancellationToken);
        MockMine(validTxs);


        // Measure ms of validating execution with GraphIdLookupTable
        EnqueueExecuteTransactions(graphFoo, "Select papers", _settings.NumEvalTransactions);
        
        stopwatch.Start();
        var validTxsBefore = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();

        var msWith = stopwatch.Elapsed.TotalMilliseconds;

        // Measure ms of execution without GraphIdLookupTable
        EnqueueExecuteTransactions(graphFoo, "Select papers", _settings.NumEvalTransactions);
        _miner.Blockchain.DisableGraphIdLookupTable = true;
        stopwatch.Reset();

        stopwatch.Start();
        var validTxsAfter = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();

        var msWithout = stopwatch.Elapsed.TotalMilliseconds;
        var validationTime = 2400;
        Console.WriteLine("Total Time");
        Console.WriteLine($"With : {msWith} ms  --  Without : {msWithout} ms");
        Console.WriteLine("Time per transaction");
        Console.WriteLine($"With : {msWith/_settings.NumEvalTransactions} ms  --  Without : {msWithout/_settings.NumEvalTransactions} ms");
        Console.WriteLine("Theoretical Block Size");
        Console.WriteLine($"With : {validationTime/(msWith/_settings.NumEvalTransactions)} --  Without : {validationTime/(msWithout/_settings.NumEvalTransactions)}");
        Assert.IsTrue(msWith < msWithout);
        Assert.IsTrue(validTxsBefore.Count == _settings.NumEvalTransactions);
        Assert.IsTrue(validTxsAfter.Count == _settings.NumEvalTransactions);
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