using NUnit.Framework;
using System.Diagnostics;
using DCR;

namespace Tests;

public class GraphCreatorTests
{
    private GraphSerializer? _graphSerializer;
    private BlockchainSerializer? _blockchainSerializer;
    private Settings? _settings;
    private Miner? _miner;

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
            IsEval = true,
            NumEvalTransactions = 5000
        };
        _miner = new Miner(logger, networkClient, _settings);
    }

    [Test]
    public void Evaluate_DequeueTransactions()
    {
        var graphJson = """{"Actor":"Foo","Activities":[{"title":"Select papers","pending":true,"included":true,"executed":false,"enabled":true},{"title":"Write introduction","pending":true,"included":true,"executed":false,"enabled":false},{"title":"Write abstract","pending":true,"included":true,"executed":false,"enabled":false},{"title":"Write conclusion","pending":true,"included":true,"executed":false,"enabled":false}],"Relations":[{"source":"Select papers","type":2,"target":"Select papers"},{"source":"Select papers","type":0,"target":"Write introduction"},{"source":"Select papers","type":0,"target":"Write abstract"},{"source":"Select papers","type":0,"target":"Write conclusion"},{"source":"Write introduction","type":1,"target":"Write abstract"},{"source":"Write conclusion","type":1,"target":"Write abstract"}]}""";

        // Enqueue Create transactions
        EnqueueTransactions(graphJson, _settings.NumEvalTransactions);

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

    private void EnqueueTransactions(string graphJson, int numTransactions) {
        for (int i = 0; i < numTransactions; i++) {
            var graph = _graphSerializer.Deserialize(graphJson);
            var tx = new Transaction("eval", DCR.Action.Create, "", graph);
            _miner.HandleTransaction(tx);
        }

    }
}