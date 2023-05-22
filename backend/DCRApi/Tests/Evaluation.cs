using NUnit.Framework;
using System.Diagnostics;
using DCR;

namespace DCRApi.Tests;

public class Evaluation
{
    private Miner _miner;
    private List<int> _blockchainSizes;
    private List<int> _blockSizes;

    [SetUp]
    public void Setup()
    {
        _blockchainSizes = new List<int> { 10, 50, 100, 500, 1000, 5000, 10000 };
        _blockSizes = new List<int> { 10, 50, 100, 500, 1000, 5000, 10000 };
        _miner = TestHelper.InitializeMiner();
    }

    [TearDown]
    public void TearDown()
    {
        TestHelper.ClearBlockchain($"blockchain{_miner.NetworkClient.ClientNode.Port}.json");
    }

    /* 
        Evaluates how GraphIdLookupTable impacts ms searching 1 block of many transactions
        for duplicate graph
    */
    [Test]
    public void Evaluate_GraphIdLookupTable_ManyTransactions_Create()
    {
        var blockchainSize = 1;
        var loopReps = 3;

        foreach (int blockSize in _blockSizes)
        {
            Run_CreateEvaluationAverage(blockchainSize, blockSize, loopReps);
        }
    }


    /* 
        Evaluates how GraphIdLookupTable impacts ms searching many blocks of 1 transaction
        for duplicate graph
    */
    [Test]
    public void Evaluate_GraphIdLookupTable_ManyBlocks_Create()
    {
        var blockSize = 1;
        var loopReps = 3;

        foreach (int blockchainSize in _blockchainSizes)
        {
            Run_CreateEvaluationAverage(blockchainSize, blockSize, loopReps);
        }
    }

    /* 
        Evaluates how GraphIdLookupTable impacts ms searching many blocks of many transactions
        for duplicate graph
    */
    [Test]
    public void Evaluate_GraphIdLookupTable_DenseBlockchain_Create()
    {
        var loopReps = 3;
        foreach (int blockchainSize in _blockchainSizes.Take(4))
        {
            foreach (int blockSize in _blockSizes.Take(3))
            {
                Run_CreateEvaluationAverage(blockchainSize, blockSize, loopReps);
            }
        }
    }

    /* 
        Evaluates how GraphIdLookupTable impacts ms searching 1 block of many transactions
        for existing graph and valid graph state when executing activity of graph
    */
    [Test]
    public void Evaluate_GraphIdLookupTable_ManyTransactions_Execute()
    {
        var blockchainSize = 1;
        var loopReps = 3;

        foreach (int blockSize in _blockSizes)
        {
            Run_ExecuteEvaluationAverage(blockchainSize, blockSize, loopReps);
        }
    }

    /* 
        Evaluates how GraphIdLookupTable impacts ms searching many blocks of 1 transaction
        for existing graph and valid graph state when executing activity of graph
    */
    [Test]
    public void Evaluate_GraphIdLookupTable_ManyBlocks_Execute()
    {
        var blockSize = 1;
        var loopReps = 3;

        foreach (int blockchainSize in _blockchainSizes)
        {
            Run_ExecuteEvaluationAverage(blockchainSize, blockSize, loopReps);
        }
    }

    /* 
        Evaluates how GraphIdLookupTable impacts ms searching many blocks of many transactions
        for existing graph and valid graph state when executing activity of graph
    */
    [Test]
    public void Evaluate_GraphIdLookupTable_DenseBlockchain_Execute()
    {
        var loopReps = 3;
        foreach (int blockchainSize in _blockchainSizes.Take(4))
        {
            foreach (int blockSize in _blockSizes.Take(3))
            {
                Run_ExecuteEvaluationAverage(blockchainSize, blockSize, loopReps);
            }
        }
    }

    private void Run_CreateEvaluationAverage(int blockchainSize, int blockSize, int loopReps)
    {
        var msWithoutAverage = 0.0;
        var msWithAverage = 0.0;
        for (int i = 0; i < loopReps; i++)
        {
            var (msWith, msWithout) = Run_CreateEvaluation(blockchainSize, blockSize);
            msWithoutAverage += msWithout;
            msWithAverage += msWith;

            // Print iteration measurements
            // PrintTestResult(msWithout, msWith, blockchainSize, blockSize);

            // Clear setup for next iteration
            TestHelper.ClearBlockchain($"blockchain{_miner.NetworkClient.ClientNode.Port}.json");
            _miner = TestHelper.InitializeMiner();
        }

        // Print average measurements
        PrintTestResult(msWithoutAverage/loopReps, msWithAverage/loopReps, blockchainSize, blockSize);
    }

    private (double, double) Run_CreateEvaluation(int blockchainSize, int blockSize)
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Set up blockchain
        for (int i = 0; i < blockchainSize; i++) {
            for (int j = 0; j < blockSize; j++) {
                var fillerGraph = TestHelper.CreatePaperGraph(Guid.NewGuid().ToString());
                TestHelper.EnqueueCreateTransactions(_miner, fillerGraph, 1);
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

        Assert.AreEqual(1, validTxsBefore.Count);
        Assert.AreEqual(1, validTxsAfter.Count);

        return (msWith, msWithout);
    }

    private void Run_ExecuteEvaluationAverage(int blockchainSize, int blockSize, int loopReps)
    {
        var msWithoutAverage = 0.0;
        var msWithAverage = 0.0;
        for (int i = 0; i < loopReps; i++)
        {
            var (msWith, msWithout) = Run_ExecuteEvaluation(blockchainSize, blockSize);
            msWithoutAverage += msWithout;
            msWithAverage += msWith;

            // Print iteration measurements
            // PrintTestResult(msWithout, msWith, blockchainSize, blockSize);

            // Clear setup for next iteration
            TestHelper.ClearBlockchain($"blockchain{_miner.NetworkClient.ClientNode.Port}.json");
            _miner = TestHelper.InitializeMiner();
        }

        // Print average measurements
        PrintTestResult(msWithoutAverage/loopReps, msWithAverage/loopReps, blockchainSize, blockSize);
    }

    private (double, double) Run_ExecuteEvaluation(int blockchainSize, int blockSize)
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var graphFoo = TestHelper.CreatePaperGraph("foo");

        // Set up blockchain
        TestHelper.EnqueueCreateTransactions(_miner, graphFoo, 1);
        for (int i = 0; i < blockchainSize; i++) {
            for (int j = 0; j < blockSize; j++) {
                var fillerGraph = TestHelper.CreatePaperGraph(Guid.NewGuid().ToString());
                TestHelper.EnqueueCreateTransactions(_miner, fillerGraph, 1);
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

        Assert.AreEqual(1, validTxsBefore.Count);
        Assert.AreEqual(1, validTxsAfter.Count);

        return (msWith, msWithout);
    }

    private void PrintTestResult(double msWithout, double msWith, int blockchainSize, int blockSize)
    {
        Console.WriteLine($"{blockchainSize} Blocks, {blockSize} Transactions: {msWithout} ms vs {msWith} ms");
    }
}