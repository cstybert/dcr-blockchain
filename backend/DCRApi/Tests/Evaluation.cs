using NUnit.Framework;
using System.Diagnostics;
using DCR;

namespace DCRApi.Tests;

public class Evaluation
{
    private Settings _settings;
    private Miner _miner;
    private int _sizeOfBlockchain;
    private int _sizeOfBlock;

    [SetUp]
    public void Setup()
    {
        var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<Miner>();
        var networkClient = new NetworkClient("localhost", 4300);
        _settings = new Settings() {
            TimeToSleep = 0,
            SizeOfBlocks = int.MaxValue,
            NumberNeighbours = 1,
            Difficulty = 0,
            IsEval = true
        };
        _sizeOfBlockchain = 1000;
        _sizeOfBlock = 10000;
        _miner = new Miner(logger, networkClient, _settings);
    }

    /* 
        Evaluates how GraphIdLookupTable impacts ms searching 1 block of _sizeOfBlock transactions
        for duplicate graph
    */
    [Test]
    public void Evaluate_GraphIdLookupTable_ManyTransactions_Create()
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Set up blockchain
        for (int i = 0; i < _sizeOfBlock; i++) {
            var fillerGraph = TestHelper.CreatePaperGraph(Guid.NewGuid().ToString());
            TestHelper.EnqueueCreateTransactionsWithId(_miner, i.ToString(), fillerGraph, 1);
        }
        var validTxs = _miner.DequeueTransactions(cancellationToken);
        TestHelper.MockMine(_miner, validTxs);
        
        var graph = DCREngine.Tests.TestHelper.CreateMeetingGraph();
        // Measure ms of validating create with GraphIdLookupTable
        TestHelper.EnqueueCreateTransactions(_miner, graph, 1);
        
        stopwatch.Start();
        var validTxsBefore = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();
        var msWith = stopwatch.Elapsed.TotalMilliseconds;

        // Measure ms of execution without GraphIdLookupTable
        TestHelper.EnqueueCreateTransactions(_miner, graph, 1);
        _miner.Blockchain.DisableGraphIdLookupTable = true;
        stopwatch.Reset();

        stopwatch.Start();
        var validTxsAfter = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();
        var msWithout = stopwatch.Elapsed.TotalMilliseconds;

        PrintTestResult(msWith, msWithout, 1, _sizeOfBlock);

        Assert.AreEqual(1, validTxsBefore.Count);
        Assert.AreEqual(1, validTxsAfter.Count);
        Assert.IsTrue(msWith < msWithout);
    }

    /* 
        Evaluates how GraphIdLookupTable impacts ms searching 1 block of _sizeOfBlock transactions
        for existing graph and valid graph state when executing activity of graph
    */
    [Test]
    public void Evaluate_GraphIdLookupTable_ManyTransactions_Execute()
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var graphFoo = TestHelper.CreatePaperGraph("foo");

        // Set up blockchain
        TestHelper.EnqueueCreateTransactions(_miner, graphFoo, 1);
        for (int i = 0; i < _sizeOfBlock; i++) {
            var fillerGraph = TestHelper.CreatePaperGraph(Guid.NewGuid().ToString());
            TestHelper.EnqueueCreateTransactionsWithId(_miner, i.ToString(), fillerGraph, 1);
        }
        var validTxs = _miner.DequeueTransactions(cancellationToken);
        TestHelper.MockMine(_miner, validTxs);

        // Measure ms of validating execution with GraphIdLookupTable
        TestHelper.EnqueueExecuteTransactions(_miner, graphFoo, "Select papers", 1);
        
        stopwatch.Start();
        var validTxsBefore = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();
        var msWith = stopwatch.Elapsed.TotalMilliseconds;

        // Measure ms of execution without GraphIdLookupTable
        TestHelper.EnqueueExecuteTransactions(_miner, graphFoo, "Select papers", 1);
        _miner.Blockchain.DisableGraphIdLookupTable = true;
        stopwatch.Reset();

        stopwatch.Start();
        var validTxsAfter = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();
        var msWithout = stopwatch.Elapsed.TotalMilliseconds;

        PrintTestResult(msWith, msWithout, 1, _sizeOfBlock);

        Assert.AreEqual(1, validTxsBefore.Count);
        Assert.AreEqual(1, validTxsAfter.Count);
        Assert.IsTrue(msWith < msWithout);
    }

    /* 
        Evaluates how GraphIdLookupTable impacts ms searching _sizeOfBlockchain blocks of 1 transaction
        for duplicate graph
    */
    [Test]
    public void Evaluate_GraphIdLookupTable_ManyBlocks_Create()
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Set up blockchain
        for (int i = 0; i < _sizeOfBlockchain; i++) {
            var fillerGraph = TestHelper.CreatePaperGraph(Guid.NewGuid().ToString());
            TestHelper.EnqueueCreateTransactionsWithId(_miner, i.ToString(), fillerGraph, 1);
        }
        var validTxs = _miner.DequeueTransactions(cancellationToken);
        foreach (Transaction tx in validTxs) {
            TestHelper.MockMine(_miner, new List<Transaction>{ tx });
        }
        
        var graph = DCREngine.Tests.TestHelper.CreateMeetingGraph();
        // Measure ms of validating create with GraphIdLookupTable
        TestHelper.EnqueueCreateTransactions(_miner, graph, 1);
        
        stopwatch.Start();
        var validTxsBefore = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();
        var msWith = stopwatch.Elapsed.TotalMilliseconds;

        // Measure ms of execution without GraphIdLookupTable
        TestHelper.EnqueueCreateTransactions(_miner, graph, 1);
        _miner.Blockchain.DisableGraphIdLookupTable = true;
        stopwatch.Reset();

        stopwatch.Start();
        var validTxsAfter = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();
        var msWithout = stopwatch.Elapsed.TotalMilliseconds;

        PrintTestResult(msWith, msWithout, _sizeOfBlockchain, 1);
       
        Assert.AreEqual(1, validTxsBefore.Count);
        Assert.AreEqual(1, validTxsAfter.Count);
        Assert.IsTrue(msWith < msWithout);
    }

    /* 
        Evaluates how GraphIdLookupTable impacts ms searching _sizeOfBlockchain blocks of 1 transaction
        for existing graph and valid graph state when executing activity of graph
    */
    [Test]
    public void Evaluate_GraphIdLookupTable_ManyBlocks_Execute()
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var graphFoo = TestHelper.CreatePaperGraph("foo");

        // Set up blockchain
        TestHelper.EnqueueCreateTransactions(_miner, graphFoo, 1);
        for (int i = 0; i < _sizeOfBlockchain; i++) {
            var fillerGraph = TestHelper.CreatePaperGraph(Guid.NewGuid().ToString());
            TestHelper.EnqueueCreateTransactionsWithId(_miner, i.ToString(), fillerGraph, 1);
        }
        var validTxs = _miner.DequeueTransactions(cancellationToken);
        foreach (Transaction tx in validTxs) {
            TestHelper.MockMine(_miner, new List<Transaction>{ tx });
        }

        // Measure ms of validating execution with GraphIdLookupTable
        TestHelper.EnqueueExecuteTransactions(_miner, graphFoo, "Select papers", 1);
        
        stopwatch.Start();
        var validTxsBefore = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();
        var msWith = stopwatch.Elapsed.TotalMilliseconds;

        // Measure ms of execution without GraphIdLookupTable
        TestHelper.EnqueueExecuteTransactions(_miner, graphFoo, "Select papers", 1);
        _miner.Blockchain.DisableGraphIdLookupTable = true;
        stopwatch.Reset();

        stopwatch.Start();
        var validTxsAfter = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();
        var msWithout = stopwatch.Elapsed.TotalMilliseconds;

        PrintTestResult(msWith, msWithout, _sizeOfBlockchain, 1);

        Assert.AreEqual(1, validTxsBefore.Count);
        Assert.AreEqual(1, validTxsAfter.Count);
        Assert.IsTrue(msWith < msWithout);
    }

    /* 
        Evaluates how GraphIdLookupTable impacts ms searching _sizeOfBlockchain blocks of _sizeOfBlock transactions
        for duplicate graph
    */
    [Test]
    public void Evaluate_GraphIdLookupTable_DenseBlockchain_Create()
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Set up blockchain
        for (int i = 0; i < _sizeOfBlockchain; i++) {
            for (int j = 0; j < _sizeOfBlock; j++) {
                var fillerGraph = TestHelper.CreatePaperGraph(Guid.NewGuid().ToString());
                TestHelper.EnqueueCreateTransactionsWithId(_miner, i.ToString(), fillerGraph, 1);
            }
            var validTxs = _miner.DequeueTransactions(cancellationToken);
            TestHelper.MockMine(_miner, validTxs);
        }

        var graph = DCREngine.Tests.TestHelper.CreateMeetingGraph();
        // Measure ms of validating create with GraphIdLookupTable
        TestHelper.EnqueueCreateTransactions(_miner, graph, 1);
        
        stopwatch.Start();
        var validTxsBefore = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();
        var msWith = stopwatch.Elapsed.TotalMilliseconds;

        // Measure ms of execution without GraphIdLookupTable
        TestHelper.EnqueueCreateTransactions(_miner, graph, 1);
        _miner.Blockchain.DisableGraphIdLookupTable = true;
        stopwatch.Reset();

        stopwatch.Start();
        var validTxsAfter = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();
        var msWithout = stopwatch.Elapsed.TotalMilliseconds;

        PrintTestResult(msWith, msWithout);

        Assert.AreEqual(1, validTxsBefore.Count);
        Assert.AreEqual(1, validTxsAfter.Count);
        Assert.IsTrue(msWith < msWithout);
    }

    /* 
        Evaluates how GraphIdLookupTable impacts ms searching _sizeOfBlockchain blocks of _sizeOfBlock transactions
        for existing graph and valid graph state when executing activity of graph
    */
    [Test]
    public void Evaluate_GraphIdLookupTable_DenseBlockchain_Execute()
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var graphFoo = TestHelper.CreatePaperGraph("foo");

        // Set up blockchain
        TestHelper.EnqueueCreateTransactions(_miner, graphFoo, 1);
        for (int i = 0; i < _sizeOfBlockchain; i++) {
            for (int j = 0; j < _sizeOfBlock; j++) {
                var fillerGraph = TestHelper.CreatePaperGraph(Guid.NewGuid().ToString());
                TestHelper.EnqueueCreateTransactionsWithId(_miner, i.ToString(), fillerGraph, 1);
            }
            var validTxs = _miner.DequeueTransactions(cancellationToken);
            TestHelper.MockMine(_miner, validTxs);
        }

        var graph = DCREngine.Tests.TestHelper.CreateMeetingGraph();
        // Measure ms of validating create with GraphIdLookupTable
        TestHelper.EnqueueExecuteTransactions(_miner, graphFoo, "Select papers", 1);
        
        stopwatch.Start();
        var validTxsBefore = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();
        var msWith = stopwatch.Elapsed.TotalMilliseconds;

        // Measure ms of execution without GraphIdLookupTable
        TestHelper.EnqueueExecuteTransactions(_miner, graphFoo, "Select papers", 1);
        _miner.Blockchain.DisableGraphIdLookupTable = true;
        stopwatch.Reset();

        stopwatch.Start();
        var validTxsAfter = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();
        var msWithout = stopwatch.Elapsed.TotalMilliseconds;

        PrintTestResult(msWith, msWithout);

        Assert.AreEqual(1, validTxsBefore.Count);
        Assert.AreEqual(1, validTxsAfter.Count);
        Assert.IsTrue(msWith < msWithout);
    }

    private void PrintTestResult(double msWith, double msWithout)
    {
        PrintTestResult(msWith, msWithout, _sizeOfBlockchain, _sizeOfBlock);
    }

    private void PrintTestResult(double msWith, double msWithout, int sizeOfBlockchain, int sizeOfBlock)
    {
        Console.WriteLine($"(Test Parameters) {sizeOfBlockchain} blocks of {sizeOfBlock} transactions, totalling {sizeOfBlockchain*sizeOfBlock} transactions");
        Console.WriteLine($"(Elapsed Time) With: {msWith} ms  --  Without: {msWithout} ms");
    }
}