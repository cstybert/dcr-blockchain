using NUnit.Framework;
using Models;

namespace DCREngine.Tests;

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
    public void Create_MedicineGraph()
    {
        var graph = TestHelper.CreateMedicineGraph();

        Assert.AreEqual(4, graph.Activities.Count);
        Assert.AreEqual(10, graph.Relations.Count);

        TestHelper.AssertActivityStatuses(graph, "Prescribe medicine", true, false, false, true);
        TestHelper.AssertActivityStatuses(graph, "Sign prescription", false, false, false, true);
        TestHelper.AssertActivityStatuses(graph, "Reject prescription", false, false, false, true);
        TestHelper.AssertActivityStatuses(graph, "Administer medicine", false, false, false, true);
    }

    [Test]
    public void Create_PaperGraph()
    {
        var graph = TestHelper.CreatePaperGraph();

        Assert.AreEqual(4, graph.Activities.Count);
        Assert.AreEqual(6, graph.Relations.Count);

        TestHelper.AssertActivityStatuses(graph, "Select papers", true, false, true, true);
        TestHelper.AssertActivityStatuses(graph, "Write introduction", false, false, true, true);
        TestHelper.AssertActivityStatuses(graph, "Write conclusion", false, false, true, true);
        TestHelper.AssertActivityStatuses(graph, "Write abstract", false, false, true, true);
    }

    [Test]
    public void Create_MeetingGraph()
    {
        var graph = TestHelper.CreateMeetingGraph();

        Assert.AreEqual(5, graph.Activities.Count);
        Assert.AreEqual(13, graph.Relations.Count);
        
        TestHelper.AssertActivityStatuses(graph, "Propose - E1", true, false, false, true);
        TestHelper.AssertActivityStatuses(graph, "Propose - E2", false, false, false, true);
        TestHelper.AssertActivityStatuses(graph, "Accept - E1", false, false, false, false);
        TestHelper.AssertActivityStatuses(graph, "Accept - E2", false, false, false, false);
        TestHelper.AssertActivityStatuses(graph, "Hold meeting", false, false, true, false);
    }
}
