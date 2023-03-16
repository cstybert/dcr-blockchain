using NUnit.Framework;
using System;
using System.Collections.Generic;
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
        var graph = _graphCreator.Create("");

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

    [Test]
    public void Parse_SimpleGraph()
    {
        var input = "\"ActivityA\", \"ActivityB\", \"ActivityA\"-->*\"ActivityB\"";

        var graph = _graphCreator.Create(input);

        Assert.AreEqual(2, graph.Activities.Count);
        Assert.AreEqual("ActivityA", graph.Activities[0].Title);
        Assert.AreEqual("ActivityB", graph.Activities[1].Title);

        Assert.AreEqual(1, graph.Relations.Count);
        Assert.AreEqual(RelationType.CONDITION, graph.Relations[0].Type);
        Assert.AreEqual("ActivityA", graph.Relations[0].Source);
        Assert.AreEqual("ActivityB", graph.Relations[0].Target);
    }
}
