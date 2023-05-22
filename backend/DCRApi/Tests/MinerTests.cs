using NUnit.Framework;
using DCR;

namespace DCRApi.Tests;

public class MinerTests
{
    private Miner _miner;

    [SetUp]
    public void Setup()
    {
        _miner = TestHelper.InitializeMiner();
    }

    [TearDown]
    public void TearDown()
    {
        TestHelper.ClearBlockchain($"blockchain{_miner.NetworkClient.ClientNode.Port}.json");
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