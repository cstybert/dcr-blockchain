using NUnit.Framework;
using System;
using Models;
using Business;

namespace Tests;

public class GraphCreatorTests
{
    private GraphCreator graphCreator = new GraphCreator();

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Create_EmptyGraph()
    {
        var graph = graphCreator.create("");

        Assert.AreEqual(0, graph.Activities.Count);
        Assert.AreEqual(0, graph.Relations.Count);
        Assert.AreEqual(false, graph.Executing);
    }
}
