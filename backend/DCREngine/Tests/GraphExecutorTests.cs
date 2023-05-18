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
    public void Execute_SimpleGraph()
    {
        var act1 = new Activity("Select papers", true);
        var act2 = new Activity("Write introduction", true);
        var act3 = new Activity("Write abstract", true);
        var act4 = new Activity("Write conclusion", true);
        var activities = new List<Activity> {act1, act2, act3, act4};

        var rel1 = new Relation(RelationType.EXCLUSION, act1, act1);
        var rel2 = new Relation(RelationType.CONDITION, act1, act2);
        var rel3 = new Relation(RelationType.CONDITION, act1, act3);
        var rel4 = new Relation(RelationType.CONDITION, act1, act4);
        var rel5 = new Relation(RelationType.RESPONSE, act2, act3);
        var rel6 = new Relation(RelationType.RESPONSE, act4, act3);
        var relations = new List<Relation> {rel1, rel2, rel3, rel4, rel5, rel6};

        var graph = _graphCreator.Create(activities, relations);
        Assert.AreEqual(4, graph.Activities.Count);
        Assert.AreEqual(6, graph.Relations.Count);

        Assert.AreEqual(true, act1.Pending);
        Assert.AreEqual(true, act1.Included);
        Assert.AreEqual(false, act1.Executed);
        Assert.AreEqual(true, act1.Enabled);

        Assert.AreEqual(true, act2.Pending);
        Assert.AreEqual(true, act2.Included);
        Assert.AreEqual(false, act2.Executed);
        Assert.AreEqual(false, act2.Enabled);

        Assert.AreEqual(true, act3.Pending);
        Assert.AreEqual(true, act3.Included);
        Assert.AreEqual(false, act3.Executed);
        Assert.AreEqual(false, act3.Enabled);

        Assert.AreEqual(true, act4.Pending);
        Assert.AreEqual(true, act4.Included);
        Assert.AreEqual(false, act4.Executed);
        Assert.AreEqual(false, act4.Enabled);

        _graphExecutor.Execute(graph, "Select papers");

        Assert.AreEqual(false, act1.Pending);
        Assert.AreEqual(false, act1.Included);
        Assert.AreEqual(true, act1.Executed);
        Assert.AreEqual(false, act1.Enabled);

        Assert.AreEqual(true, act2.Pending);
        Assert.AreEqual(true, act2.Included);
        Assert.AreEqual(false, act2.Executed);
        Assert.AreEqual(true, act2.Enabled);

        Assert.AreEqual(true, act3.Pending);
        Assert.AreEqual(true, act3.Included);
        Assert.AreEqual(false, act3.Executed);
        Assert.AreEqual(true, act3.Enabled);

        Assert.AreEqual(true, act4.Pending);
        Assert.AreEqual(true, act4.Included);
        Assert.AreEqual(false, act4.Executed);
        Assert.AreEqual(true, act4.Enabled);

        _graphExecutor.Execute(graph, "Write introduction");

        Assert.AreEqual(false, act1.Pending);
        Assert.AreEqual(false, act1.Included);
        Assert.AreEqual(true, act1.Executed);
        Assert.AreEqual(false, act1.Enabled);

        Assert.AreEqual(false, act2.Pending);
        Assert.AreEqual(true, act2.Included);
        Assert.AreEqual(true, act2.Executed);
        Assert.AreEqual(true, act2.Enabled);

        Assert.AreEqual(true, act3.Pending);
        Assert.AreEqual(true, act3.Included);
        Assert.AreEqual(false, act3.Executed);
        Assert.AreEqual(true, act3.Enabled);

        Assert.AreEqual(true, act4.Pending);
        Assert.AreEqual(true, act4.Included);
        Assert.AreEqual(false, act4.Executed);
        Assert.AreEqual(true, act4.Enabled);

        _graphExecutor.Execute(graph, "Write abstract");

        Assert.AreEqual(false, act1.Pending);
        Assert.AreEqual(false, act1.Included);
        Assert.AreEqual(true, act1.Executed);
        Assert.AreEqual(false, act1.Enabled);

        Assert.AreEqual(false, act2.Pending);
        Assert.AreEqual(true, act2.Included);
        Assert.AreEqual(true, act2.Executed);
        Assert.AreEqual(true, act2.Enabled);

        Assert.AreEqual(false, act3.Pending);
        Assert.AreEqual(true, act3.Included);
        Assert.AreEqual(true, act3.Executed);
        Assert.AreEqual(true, act3.Enabled);

        Assert.AreEqual(true, act4.Pending);
        Assert.AreEqual(true, act4.Included);
        Assert.AreEqual(false, act4.Executed);
        Assert.AreEqual(true, act4.Enabled);
        
        _graphExecutor.Execute(graph, "Write conclusion");

        Assert.AreEqual(false, act1.Pending);
        Assert.AreEqual(false, act1.Included);
        Assert.AreEqual(true, act1.Executed);
        Assert.AreEqual(false, act1.Enabled);

        Assert.AreEqual(false, act2.Pending);
        Assert.AreEqual(true, act2.Included);
        Assert.AreEqual(true, act2.Executed);
        Assert.AreEqual(true, act2.Enabled);

        Assert.AreEqual(true, act3.Pending);
        Assert.AreEqual(true, act3.Included);
        Assert.AreEqual(true, act3.Executed);
        Assert.AreEqual(true, act3.Enabled);

        Assert.AreEqual(false, act4.Pending);
        Assert.AreEqual(true, act4.Included);
        Assert.AreEqual(true, act4.Executed);
        Assert.AreEqual(true, act4.Enabled);

        _graphExecutor.Execute(graph, "Write abstract");

        Assert.AreEqual(false, act1.Pending);
        Assert.AreEqual(false, act1.Included);
        Assert.AreEqual(true, act1.Executed);
        Assert.AreEqual(false, act1.Enabled);

        Assert.AreEqual(false, act2.Pending);
        Assert.AreEqual(true, act2.Included);
        Assert.AreEqual(true, act2.Executed);
        Assert.AreEqual(true, act2.Enabled);

        Assert.AreEqual(false, act3.Pending);
        Assert.AreEqual(true, act3.Included);
        Assert.AreEqual(true, act3.Executed);
        Assert.AreEqual(true, act3.Enabled);

        Assert.AreEqual(false, act4.Pending);
        Assert.AreEqual(true, act4.Included);
        Assert.AreEqual(true, act4.Executed);
        Assert.AreEqual(true, act4.Enabled);

        //Console.WriteLine(JsonConvert.SerializeObject(graph, Formatting.Indented));
    }
}
