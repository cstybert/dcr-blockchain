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
}
