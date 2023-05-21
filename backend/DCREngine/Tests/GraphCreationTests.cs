using NUnit.Framework;
using Models;

namespace Tests;

public class GraphCreationTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Create_EmptyGraph()
    {
        var graph = new Graph();

        Assert.AreEqual(0, graph.Activities.Count);
        Assert.AreEqual(0, graph.Relations.Count);
    }

    [Test]
    public void Create_SimpleGraph()
    {
        var src = new Activity("A");
        var trgt = new Activity("B");
        var activities = new List<Activity> {src, trgt};
        var relations = new List<Relation> {new Relation(RelationType.CONDITION, src, trgt)};

        var graph = new Graph(activities, relations);

        Assert.AreEqual(2, graph.Activities.Count);
        Assert.AreEqual(1, graph.Relations.Count);
        Assert.AreEqual(true, src.Enabled);
        Assert.AreEqual(false, trgt.Enabled);
    }

    [Test]
    public void Create_PaperGraph()
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
    }
}
