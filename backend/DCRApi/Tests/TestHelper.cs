using DCR;
using Models;

namespace DCRApi.Tests;

public static class TestHelper
{
    public static Miner InitializeMiner()
    {
        var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<Miner>();
        var networkClient = new NetworkClient("localhost", 4300);
        var settings = new Settings() {
            TimeToSleep = 0,
            SizeOfBlocks = int.MaxValue,
            NumberNeighbours = 1,
            Difficulty = 0,
            IsEval = true
        };

        var miner = new Miner(logger, networkClient, settings);
        return miner;
    }

    public static void EnqueueCreateTransactions(Miner miner, Graph graph, int numTransactions) {
        for (int i = 0; i < numTransactions; i++) {
            var tx = new Transaction("1", DCR.Action.Create, "", graph);
            miner.HandleTransaction(tx);
        }
    }

    public static void EnqueueExecuteTransactions(Miner miner, Graph graph, string executeActivity, int numTransactions) {
        for (int i = 0; i < numTransactions; i++) {
            var graphToUpdate = miner.Blockchain.DeepCopyGraph(graph);
            graphToUpdate.Execute(executeActivity);
            var tx = new Transaction("1", DCR.Action.Update, executeActivity, graphToUpdate);
            miner.HandleTransaction(tx);
        }
    }

    public static void MockMine(Miner miner, List<Transaction> txs) {
        var block = new Block(txs) {Index = miner.Blockchain.Chain.Count};
        miner.Blockchain.Append(block);
    }

    public static Graph CreatePaperGraph(string graphId)
    {
        var graph = DCREngine.Tests.TestHelper.CreatePaperGraph();
        graph.Id = graphId;
        return graph;
    }

    public static Graph CreateMeetingGraph(string graphId)
    {
        var graph = DCREngine.Tests.TestHelper.CreateMeetingGraph();
        graph.Id = graphId;
        return graph;
    }

    public static void ClearBlockchain(string blockchainFilename)
    {
        if (System.IO.File.Exists(blockchainFilename))
        {
            var fullPath = System.IO.Path.GetFullPath(blockchainFilename);
            System.IO.File.Delete(fullPath);
        }
    }
}