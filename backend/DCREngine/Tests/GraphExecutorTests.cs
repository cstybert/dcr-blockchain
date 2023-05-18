using NUnit.Framework;
using Models;
using Business;
namespace Tests;

public class GraphExecutorTests
{
    private GraphCreator _graphCreator = new GraphCreator();
    private GraphExecutor _graphExecutor = new GraphExecutor();

    [SetUp]
    public void Setup()
    {
    }

    // TEST: Example from REBS 2020 - Lecture 1, slide 31
    [Test]
    public void Execute_PaperGraph()
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

        var graph = _graphCreator.Create(activities, relations);
        Assert.AreEqual(4, graph.Activities.Count);
        Assert.AreEqual(6, graph.Relations.Count);

        Assert.AreEqual(true, selectPapers.Pending);
        Assert.AreEqual(true, selectPapers.Included);
        Assert.AreEqual(false, selectPapers.Executed);
        Assert.AreEqual(true, selectPapers.Enabled);

        Assert.AreEqual(true, writeIntroduction.Pending);
        Assert.AreEqual(true, writeIntroduction.Included);
        Assert.AreEqual(false, writeIntroduction.Executed);
        Assert.AreEqual(false, writeIntroduction.Enabled);

        Assert.AreEqual(true, writeAbstract.Pending);
        Assert.AreEqual(true, writeAbstract.Included);
        Assert.AreEqual(false, writeAbstract.Executed);
        Assert.AreEqual(false, writeAbstract.Enabled);

        Assert.AreEqual(true, writeConclusion.Pending);
        Assert.AreEqual(true, writeConclusion.Included);
        Assert.AreEqual(false, writeConclusion.Executed);
        Assert.AreEqual(false, writeConclusion.Enabled);

        _graphExecutor.Execute(graph, selectPapers.Title);

        Assert.AreEqual(false, selectPapers.Pending);
        Assert.AreEqual(false, selectPapers.Included);
        Assert.AreEqual(true, selectPapers.Executed);
        Assert.AreEqual(false, selectPapers.Enabled);

        Assert.AreEqual(true, writeIntroduction.Pending);
        Assert.AreEqual(true, writeIntroduction.Included);
        Assert.AreEqual(false, writeIntroduction.Executed);
        Assert.AreEqual(true, writeIntroduction.Enabled);

        Assert.AreEqual(true, writeAbstract.Pending);
        Assert.AreEqual(true, writeAbstract.Included);
        Assert.AreEqual(false, writeAbstract.Executed);
        Assert.AreEqual(true, writeAbstract.Enabled);

        Assert.AreEqual(true, writeConclusion.Pending);
        Assert.AreEqual(true, writeConclusion.Included);
        Assert.AreEqual(false, writeConclusion.Executed);
        Assert.AreEqual(true, writeConclusion.Enabled);

        _graphExecutor.Execute(graph, writeIntroduction.Title);

        Assert.AreEqual(false, selectPapers.Pending);
        Assert.AreEqual(false, selectPapers.Included);
        Assert.AreEqual(true, selectPapers.Executed);
        Assert.AreEqual(false, selectPapers.Enabled);

        Assert.AreEqual(false, writeIntroduction.Pending);
        Assert.AreEqual(true, writeIntroduction.Included);
        Assert.AreEqual(true, writeIntroduction.Executed);
        Assert.AreEqual(true, writeIntroduction.Enabled);

        Assert.AreEqual(true, writeAbstract.Pending);
        Assert.AreEqual(true, writeAbstract.Included);
        Assert.AreEqual(false, writeAbstract.Executed);
        Assert.AreEqual(true, writeAbstract.Enabled);

        Assert.AreEqual(true, writeConclusion.Pending);
        Assert.AreEqual(true, writeConclusion.Included);
        Assert.AreEqual(false, writeConclusion.Executed);
        Assert.AreEqual(true, writeConclusion.Enabled);

        _graphExecutor.Execute(graph, writeAbstract.Title);

        Assert.AreEqual(false, selectPapers.Pending);
        Assert.AreEqual(false, selectPapers.Included);
        Assert.AreEqual(true, selectPapers.Executed);
        Assert.AreEqual(false, selectPapers.Enabled);

        Assert.AreEqual(false, writeIntroduction.Pending);
        Assert.AreEqual(true, writeIntroduction.Included);
        Assert.AreEqual(true, writeIntroduction.Executed);
        Assert.AreEqual(true, writeIntroduction.Enabled);

        Assert.AreEqual(false, writeAbstract.Pending);
        Assert.AreEqual(true, writeAbstract.Included);
        Assert.AreEqual(true, writeAbstract.Executed);
        Assert.AreEqual(true, writeAbstract.Enabled);

        Assert.AreEqual(true, writeConclusion.Pending);
        Assert.AreEqual(true, writeConclusion.Included);
        Assert.AreEqual(false, writeConclusion.Executed);
        Assert.AreEqual(true, writeConclusion.Enabled);
        
        _graphExecutor.Execute(graph, writeConclusion.Title);

        Assert.AreEqual(false, selectPapers.Pending);
        Assert.AreEqual(false, selectPapers.Included);
        Assert.AreEqual(true, selectPapers.Executed);
        Assert.AreEqual(false, selectPapers.Enabled);

        Assert.AreEqual(false, writeIntroduction.Pending);
        Assert.AreEqual(true, writeIntroduction.Included);
        Assert.AreEqual(true, writeIntroduction.Executed);
        Assert.AreEqual(true, writeIntroduction.Enabled);

        Assert.AreEqual(true, writeAbstract.Pending);
        Assert.AreEqual(true, writeAbstract.Included);
        Assert.AreEqual(true, writeAbstract.Executed);
        Assert.AreEqual(true, writeAbstract.Enabled);

        Assert.AreEqual(false, writeConclusion.Pending);
        Assert.AreEqual(true, writeConclusion.Included);
        Assert.AreEqual(true, writeConclusion.Executed);
        Assert.AreEqual(true, writeConclusion.Enabled);

        _graphExecutor.Execute(graph, writeAbstract.Title);

        Assert.AreEqual(false, selectPapers.Pending);
        Assert.AreEqual(false, selectPapers.Included);
        Assert.AreEqual(true, selectPapers.Executed);
        Assert.AreEqual(false, selectPapers.Enabled);

        Assert.AreEqual(false, writeIntroduction.Pending);
        Assert.AreEqual(true, writeIntroduction.Included);
        Assert.AreEqual(true, writeIntroduction.Executed);
        Assert.AreEqual(true, writeIntroduction.Enabled);

        Assert.AreEqual(false, writeAbstract.Pending);
        Assert.AreEqual(true, writeAbstract.Included);
        Assert.AreEqual(true, writeAbstract.Executed);
        Assert.AreEqual(true, writeAbstract.Enabled);

        Assert.AreEqual(false, writeConclusion.Pending);
        Assert.AreEqual(true, writeConclusion.Included);
        Assert.AreEqual(true, writeConclusion.Executed);
        Assert.AreEqual(true, writeConclusion.Enabled);

        Assert.AreEqual(true, graph.Accepting);
    }

    [Test]
    public void Execute_MeetingGraph()
    {
        var proposeDU = new Activity("Propose - DU");
        var proposeDE = new Activity("Propose - DE");
        var acceptDU = new Activity("Accept - DU");
        var acceptDE = new Activity("Accept - DE");
        var holdMeeting = new Activity("Hold Meeting", true);
        var activities = new List<Activity> {proposeDU, proposeDE, acceptDU, acceptDE, holdMeeting};

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

        var graph = _graphCreator.Create(activities, relations);
        Assert.AreEqual(5, graph.Activities.Count);
        Assert.AreEqual(11, graph.Relations.Count);

        Assert.AreEqual(false, proposeDU.Pending);
        Assert.AreEqual(true, proposeDU.Included);
        Assert.AreEqual(false, proposeDU.Executed);
        Assert.AreEqual(true, proposeDU.Enabled);

        Assert.AreEqual(false, proposeDE.Pending);
        Assert.AreEqual(true, proposeDE.Included);
        Assert.AreEqual(false, proposeDE.Executed);
        Assert.AreEqual(false, proposeDE.Enabled);

        Assert.AreEqual(false, acceptDU.Pending);
        Assert.AreEqual(true, acceptDU.Included);
        Assert.AreEqual(false, acceptDU.Executed);
        Assert.AreEqual(true, acceptDU.Enabled);

        Assert.AreEqual(false, acceptDE.Pending);
        Assert.AreEqual(true, acceptDE.Included);
        Assert.AreEqual(false, acceptDE.Executed);
        Assert.AreEqual(true, acceptDE.Enabled);

        Assert.AreEqual(true, holdMeeting.Pending);
        Assert.AreEqual(true, holdMeeting.Included);
        Assert.AreEqual(false, holdMeeting.Executed);
        Assert.AreEqual(false, holdMeeting.Enabled);

        _graphExecutor.Execute(graph, proposeDU.Title);

        Assert.AreEqual(false, proposeDU.Pending);
        Assert.AreEqual(true, proposeDU.Included);
        Assert.AreEqual(true, proposeDU.Executed);
        Assert.AreEqual(true, proposeDU.Enabled);

        Assert.AreEqual(false, proposeDE.Pending);
        Assert.AreEqual(true, proposeDE.Included);
        Assert.AreEqual(false, proposeDE.Executed);
        Assert.AreEqual(true, proposeDE.Enabled);

        Assert.AreEqual(false, acceptDU.Pending);
        Assert.AreEqual(true, acceptDU.Included);
        Assert.AreEqual(false, acceptDU.Executed);
        Assert.AreEqual(true, acceptDU.Enabled);

        Assert.AreEqual(true, acceptDE.Pending);
        Assert.AreEqual(true, acceptDE.Included);
        Assert.AreEqual(false, acceptDE.Executed);
        Assert.AreEqual(true, acceptDE.Enabled);

        Assert.AreEqual(true, holdMeeting.Pending);
        Assert.AreEqual(true, holdMeeting.Included);
        Assert.AreEqual(false, holdMeeting.Executed);
        Assert.AreEqual(false, holdMeeting.Enabled);

        _graphExecutor.Execute(graph, acceptDU.Title);

        Assert.AreEqual(false, proposeDU.Pending);
        Assert.AreEqual(true, proposeDU.Included);
        Assert.AreEqual(true, proposeDU.Executed);
        Assert.AreEqual(true, proposeDU.Enabled);

        Assert.AreEqual(false, proposeDE.Pending);
        Assert.AreEqual(true, proposeDE.Included);
        Assert.AreEqual(false, proposeDE.Executed);
        Assert.AreEqual(true, proposeDE.Enabled);

        Assert.AreEqual(false, acceptDU.Pending);
        Assert.AreEqual(false, acceptDU.Included);
        Assert.AreEqual(true, acceptDU.Executed);
        Assert.AreEqual(false, acceptDU.Enabled);

        Assert.AreEqual(true, acceptDE.Pending);
        Assert.AreEqual(false, acceptDE.Included);
        Assert.AreEqual(false, acceptDE.Executed);
        Assert.AreEqual(false, acceptDE.Enabled);

        Assert.AreEqual(true, holdMeeting.Pending);
        Assert.AreEqual(true, holdMeeting.Included);
        Assert.AreEqual(false, holdMeeting.Executed);
        Assert.AreEqual(true, holdMeeting.Enabled);

        _graphExecutor.Execute(graph, proposeDE.Title);
        
        Assert.AreEqual(false, proposeDU.Pending);
        Assert.AreEqual(true, proposeDU.Included);
        Assert.AreEqual(true, proposeDU.Executed);
        Assert.AreEqual(true, proposeDU.Enabled);

        Assert.AreEqual(false, proposeDE.Pending);
        Assert.AreEqual(true, proposeDE.Included);
        Assert.AreEqual(true, proposeDE.Executed);
        Assert.AreEqual(true, proposeDE.Enabled);

        Assert.AreEqual(true, acceptDU.Pending);
        Assert.AreEqual(true, acceptDU.Included);
        Assert.AreEqual(true, acceptDU.Executed);
        Assert.AreEqual(true, acceptDU.Enabled);

        Assert.AreEqual(true, acceptDE.Pending);
        Assert.AreEqual(false, acceptDE.Included);
        Assert.AreEqual(false, acceptDE.Executed);
        Assert.AreEqual(false, acceptDE.Enabled);

        Assert.AreEqual(true, holdMeeting.Pending);
        Assert.AreEqual(true, holdMeeting.Included);
        Assert.AreEqual(false, holdMeeting.Executed);
        Assert.AreEqual(true, holdMeeting.Enabled);

        _graphExecutor.Execute(graph, acceptDU.Title);

        Assert.AreEqual(false, proposeDU.Pending);
        Assert.AreEqual(true, proposeDU.Included);
        Assert.AreEqual(true, proposeDU.Executed);
        Assert.AreEqual(true, proposeDU.Enabled);

        Assert.AreEqual(false, proposeDE.Pending);
        Assert.AreEqual(true, proposeDE.Included);
        Assert.AreEqual(true, proposeDE.Executed);
        Assert.AreEqual(true, proposeDE.Enabled);

        Assert.AreEqual(false, acceptDU.Pending);
        Assert.AreEqual(false, acceptDU.Included);
        Assert.AreEqual(true, acceptDU.Executed);
        Assert.AreEqual(false, acceptDU.Enabled);

        Assert.AreEqual(true, acceptDE.Pending);
        Assert.AreEqual(false, acceptDE.Included);
        Assert.AreEqual(false, acceptDE.Executed);
        Assert.AreEqual(false, acceptDE.Enabled);

        Assert.AreEqual(true, holdMeeting.Pending);
        Assert.AreEqual(true, holdMeeting.Included);
        Assert.AreEqual(false, holdMeeting.Executed);
        Assert.AreEqual(true, holdMeeting.Enabled);

        _graphExecutor.Execute(graph, holdMeeting.Title);

        Assert.AreEqual(false, proposeDU.Pending);
        Assert.AreEqual(true, proposeDU.Included);
        Assert.AreEqual(true, proposeDU.Executed);
        Assert.AreEqual(true, proposeDU.Enabled);

        Assert.AreEqual(false, proposeDE.Pending);
        Assert.AreEqual(true, proposeDE.Included);
        Assert.AreEqual(true, proposeDE.Executed);
        Assert.AreEqual(true, proposeDE.Enabled);

        Assert.AreEqual(false, acceptDU.Pending);
        Assert.AreEqual(false, acceptDU.Included);
        Assert.AreEqual(true, acceptDU.Executed);
        Assert.AreEqual(false, acceptDU.Enabled);

        Assert.AreEqual(true, acceptDE.Pending);
        Assert.AreEqual(false, acceptDE.Included);
        Assert.AreEqual(false, acceptDE.Executed);
        Assert.AreEqual(false, acceptDE.Enabled);

        Assert.AreEqual(false, holdMeeting.Pending);
        Assert.AreEqual(true, holdMeeting.Included);
        Assert.AreEqual(true, holdMeeting.Executed);
        Assert.AreEqual(true, holdMeeting.Enabled);

        Assert.AreEqual(true, graph.Accepting);
    }
}
