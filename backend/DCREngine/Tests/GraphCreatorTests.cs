using NUnit.Framework;
using Models;
using Business;

namespace Tests;

public class GraphCreatorTests
{
    private GraphCreator _graphCreator = new GraphCreator();

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Create_EmptyGraph()
    {
        var graph = _graphCreator.Create(new List<Activity>(), new List<Relation>());

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

        var graph = _graphCreator.Create(activities, relations);

        Assert.AreEqual(2, graph.Activities.Count);
        Assert.AreEqual(1, graph.Relations.Count);
    }
}
