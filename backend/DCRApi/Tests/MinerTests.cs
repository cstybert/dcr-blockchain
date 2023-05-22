using NUnit.Framework;
using DCR;

namespace DCRApi.Tests;

public class MinerTests
{
    private Settings _settings;
    private Miner _miner;

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
        _miner = new Miner(logger, networkClient, _settings);
    }

    /* 
        Tests that validating 1000 valid transactions results in 1000 validated transactions
    */
    [Test]
    public void Test_TransactionValidation_Valid()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var graphFoo = TestHelper.CreatePaperGraph("foo");

        TestHelper.EnqueueCreateTransactions(_miner, graphFoo, 1);
        var createTx = _miner.DequeueTransactions(cancellationToken);
        TestHelper.MockMine(_miner, createTx);

        TestHelper.EnqueueExecuteTransactions(_miner, graphFoo, "Select papers", 1000);
        var validTxs = _miner.DequeueTransactions(cancellationToken);

        Assert.AreEqual(1000, validTxs.Count());
    }

    /* 
        Tests that validating 1000 transactions targeting a non-existing graph ID results in 0 validated transactions
    */
    [Test]
    public void Test_TransactionValidation_InvalidGraph()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var graphFoo = TestHelper.CreatePaperGraph("foo");

        TestHelper.EnqueueExecuteTransactions(_miner, graphFoo, "Select papers", 1000);
        var validTxs = _miner.DequeueTransactions(cancellationToken);

        Assert.AreEqual(0, validTxs.Count());
    }

    /* 
        Tests that validating 1 transaction containing an invalid graph state results in 0 validated transactions
    */
    [Test]
    public void Test_TransactionValidation_InvalidState()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var graphFoo = TestHelper.CreatePaperGraph("foo");

        TestHelper.EnqueueCreateTransactions(_miner, graphFoo, 1);
        var createTx = _miner.DequeueTransactions(cancellationToken);
        TestHelper.MockMine(_miner, createTx);

        TestHelper.EnqueueExecuteTransactions(_miner, graphFoo, "Write abstract", 1);
        var validTxs = _miner.DequeueTransactions(cancellationToken);

        Assert.AreEqual(0, validTxs.Count());
    }
}