using NUnit.Framework;
using System.Diagnostics;
using DCR;

namespace DCRApi.Tests;

public class EvaluationCompare
{
    private Settings _settings;
    private Miner _miner;
    private int _numBlocks;
    private int _sizeOfBlock;
    private int _numEvalTransactions;

    [SetUp]
    public void Setup()
    {
        var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<Miner>();
        var networkClient = new NetworkClient("localhost", 4300);
        _settings = new Settings()
        {
            TimeToSleep = 0,
            SizeOfBlocks = int.MaxValue,
            NumberNeighbours = 1,
            Difficulty = 0,
            IsEval = true
        };
        _numBlocks = 10;
        _sizeOfBlock = 1000;
        _numEvalTransactions = 10000;
        _miner = new Miner(logger, networkClient, _settings);
    }

    private void PopulateFillerBlock(CancellationToken cancellationToken, int sizeOfBlock)
    {
        for (int i = 0; i < sizeOfBlock; i++)
        {
            var fillerGraph = TestHelper.CreatePaperGraph(Guid.NewGuid().ToString());
            TestHelper.EnqueueCreateTransactions(_miner, fillerGraph, 1);
        }
        var validTxs = _miner.DequeueTransactions(cancellationToken);
        TestHelper.MockMine(_miner, validTxs);
    }

    private void CreateInitialBlock(Models.Graph graph, CancellationToken cancellationToken)
    {
        // Set up first block containing foo graph in index 0
        TestHelper.EnqueueCreateTransactions(_miner, graph, 1);
        PopulateFillerBlock(cancellationToken, _sizeOfBlock - 1);
    }
    private void PrintResults(double ms)
    {
        var validationTime = 2400;
        Console.WriteLine($"Total Time : {ms} ms");
        Console.WriteLine($"Time per transaction {ms / _numEvalTransactions} ms");
        Console.WriteLine($"Theoretical Block Size {validationTime / (ms / _numEvalTransactions)}");
    }
    [Test]
    public void Test_Speed_Create()
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Set up blockchain
        for (int i = 0; i < _numBlocks; i++)
        {
            PopulateFillerBlock(cancellationToken, _sizeOfBlock);
        }

        var graph = DCREngine.Tests.TestHelper.CreateMeetingGraph();
        // Measure ms of validating create with GraphIdLookupTable
        TestHelper.EnqueueCreateTransactions(_miner, graph, _numEvalTransactions);

        stopwatch.Start();
        var validTxsBefore = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();
        PrintResults(stopwatch.Elapsed.TotalMilliseconds);
        Assert.IsTrue(validTxsBefore.Count == _numEvalTransactions);
        // - 1 because genesis block is empty
        Assert.AreEqual(_numBlocks,_miner.Blockchain.Chain.Count - 1);
    }
    [Test]
    public void Test_Speed_Execute()
    {
        var stopwatch = new Stopwatch();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var graphFoo = TestHelper.CreatePaperGraph("foo");
        CreateInitialBlock(graphFoo, cancellationToken);
        for (int i = 1; i < _numBlocks; i++)
        {
            PopulateFillerBlock(cancellationToken, _sizeOfBlock);
        }

        var graph = DCREngine.Tests.TestHelper.CreateMeetingGraph();
        TestHelper.EnqueueExecuteTransactions(_miner, graphFoo, "Select papers", _numEvalTransactions);
        stopwatch.Start();
        var validTxsBefore = _miner.DequeueTransactions(cancellationToken);
        stopwatch.Stop();
        PrintResults(stopwatch.Elapsed.TotalMilliseconds);
        Assert.IsTrue(validTxsBefore.Count == _numEvalTransactions);
        // - 1 because genesis block is empty
        Assert.AreEqual(_numBlocks,_miner.Blockchain.Chain.Count - 1);
    }
}