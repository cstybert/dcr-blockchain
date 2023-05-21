using NUnit.Framework;
using System.Diagnostics;
using DCR;

namespace Tests;

public class EvaluationCompare
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
        _sizeOfBlockchain = 20000;
        _sizeOfBlock = 50000;
        _miner = new Miner(logger, networkClient, _settings);
    }

    // [Test]
    // public void Evaluate_GraphIdLookupTable_ManyTransactions_Create()
    // {
    //     var stopwatch = new Stopwatch();
    //     var cancellationTokenSource = new CancellationTokenSource();
    //     var cancellationToken = cancellationTokenSource.Token;

    //     // Set up blockchain
    //     for (int i = 0; i < _sizeOfBlockchain; i++) {
    //         var fillerGraph = TestHelper.CreatePaperGraph(Guid.NewGuid().ToString());
    //         TestHelper.EnqueueCreateTransactionsWithId(_miner, i.ToString(), fillerGraph, 1);
    //     }
    //     var validTxs = _miner.DequeueTransactions(cancellationToken);
    //     TestHelper.MockMine(_miner, validTxs);
        
    //     var graph = TestHelper.CreateMeetingGraph();
    //     // Measure ms of validating create with GraphIdLookupTable
    //     TestHelper.EnqueueCreateTransactions(_miner, graph, _settings.NumEvalTransactions);
        
    //     stopwatch.Start();
    //     var validTxsBefore = _miner.DequeueTransactions(cancellationToken);
    //     stopwatch.Stop();
    //     var msWith = stopwatch.Elapsed.TotalMilliseconds;

    //     // Measure ms of execution without GraphIdLookupTable
    //     TestHelper.EnqueueCreateTransactions(_miner, graph, _settings.NumEvalTransactions);
    //     _miner.Blockchain.DisableGraphIdLookupTable = true;
    //     stopwatch.Reset();

    //     stopwatch.Start();
    //     var validTxsAfter = _miner.DequeueTransactions(cancellationToken);
    //     stopwatch.Stop();

    //     var msWithout = stopwatch.Elapsed.TotalMilliseconds;
    //     var validationTime = 2400;
    //     Console.WriteLine("Total Time");
    //     Console.WriteLine($"With : {msWith} ms  --  Without : {msWithout} ms");
    //     Console.WriteLine("Time per transaction");
    //     Console.WriteLine($"With : {msWith/_settings.NumEvalTransactions} ms  --  Without : {msWithout/_settings.NumEvalTransactions} ms");
    //     Console.WriteLine("Theoretical Block Size");
    //     Console.WriteLine($"With : {validationTime/(msWith/_settings.NumEvalTransactions)} --  Without : {validationTime/(msWithout/_settings.NumEvalTransactions)}");
    //     Assert.IsTrue(msWith < msWithout);
    //     Assert.IsTrue(validTxsBefore.Count == _settings.NumEvalTransactions);
    //     Assert.IsTrue(validTxsAfter.Count == _settings.NumEvalTransactions);
    // }

    // [Test]
    // public void Evaluate_GraphIdLookupTable_ManyTransactions_Execute()
    // {
    //     var stopwatch = new Stopwatch();
    //     var cancellationTokenSource = new CancellationTokenSource();
    //     var cancellationToken = cancellationTokenSource.Token;
    //     var graphFoo = TestHelper.CreatePaperGraph("foo");

    //     // Set up blockchain
    //     TestHelper.EnqueueCreateTransactions(_miner, graphFoo, 1);
    //     for (int i = 0; i < _sizeOfBlockchain; i++) {
    //         var fillerGraph = TestHelper.CreatePaperGraph(Guid.NewGuid().ToString());
    //         TestHelper.EnqueueCreateTransactionsWithId(_miner, i.ToString(), fillerGraph, 1);
    //     }
    //     var validTxs = _miner.DequeueTransactions(cancellationToken);
    //     TestHelper.MockMine(_miner, validTxs);


    //     // Measure ms of validating execution with GraphIdLookupTable
    //     TestHelper.EnqueueExecuteTransactions(_miner, graphFoo, "Select papers", _settings.NumEvalTransactions);
        
    //     stopwatch.Start();
    //     var validTxsBefore = _miner.DequeueTransactions(cancellationToken);
    //     stopwatch.Stop();

    //     var msWith = stopwatch.Elapsed.TotalMilliseconds;

    //     // Measure ms of execution without GraphIdLookupTable
    //     TestHelper.EnqueueExecuteTransactions(_miner, graphFoo, "Select papers", _settings.NumEvalTransactions);
    //     _miner.Blockchain.DisableGraphIdLookupTable = true;
    //     stopwatch.Reset();

    //     stopwatch.Start();
    //     var validTxsAfter = _miner.DequeueTransactions(cancellationToken);
    //     stopwatch.Stop();

    //     var msWithout = stopwatch.Elapsed.TotalMilliseconds;
    //     var validationTime = 2400;
    //     Console.WriteLine("Total Time");
    //     Console.WriteLine($"With : {msWith} ms  --  Without : {msWithout} ms");
    //     Console.WriteLine("Time per transaction");
    //     Console.WriteLine($"With : {msWith/_settings.NumEvalTransactions} ms  --  Without : {msWithout/_settings.NumEvalTransactions} ms");
    //     Console.WriteLine("Theoretical Block Size");
    //     Console.WriteLine($"With : {validationTime/(msWith/_settings.NumEvalTransactions)} --  Without : {validationTime/(msWithout/_settings.NumEvalTransactions)}");
    //     Assert.IsTrue(msWith < msWithout);
    //     Assert.IsTrue(validTxsBefore.Count == _settings.NumEvalTransactions);
    //     Assert.IsTrue(validTxsAfter.Count == _settings.NumEvalTransactions);
    // }
}