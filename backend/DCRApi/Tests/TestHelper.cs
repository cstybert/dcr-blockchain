using DCR;
using Models;

namespace Tests;

public static class TestHelper
{
    public static void EnqueueCreateTransactionsWithId(Miner miner, string transactionId, Graph graph, int numTransactions) {
        for (int i = 0; i < numTransactions; i++) {
            var tx = new Transaction(transactionId, DCR.Action.Create, "", graph);
            miner.HandleTransaction(tx);
        }
    }

    public static void EnqueueCreateTransactions(Miner miner, Graph graph, int numTransactions) {
        EnqueueCreateTransactionsWithId(miner, "eval", graph, numTransactions);
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
        var selectPapers = new Activity("Select papers", true);
        var writeIntroduction = new Activity("Write introduction", true);
        var writeAbstract = new Activity("Write abstract", true);
        var writeConclusion = new Activity("Write conclusion", true);
        var activities = new List<Activity> {selectPapers, writeIntroduction, writeAbstract, writeConclusion};

        var rel1 = new Relation(RelationType.EXCLUSION, selectPapers, selectPapers);
        var rel2 = new Relation(RelationType.CONDITION, selectPapers, writeIntroduction);
        var rel3 = new Relation(RelationType.CONDITION, selectPapers, writeAbstract);
        var rel4 = new Relation(RelationType.CONDITION, selectPapers, writeConclusion);
        var rel5 = new Relation(RelationType.RESPONSE, writeIntroduction, writeAbstract);
        var rel6 = new Relation(RelationType.RESPONSE, writeConclusion, writeAbstract);
        var relations = new List<Relation> {rel1, rel2, rel3, rel4, rel5, rel6};

        var graph = new Graph(activities, relations);
        graph.Id = graphId;
        return graph;
    }

    public static Graph CreateMeetingGraph()
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