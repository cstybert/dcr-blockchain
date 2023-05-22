using Models;
using NUnit.Framework;

namespace DCREngine.Tests;

public static class TestHelper
{
    public static Graph CreateMedicineGraph()
    {
        var prescribeMedicine = new Activity("Prescribe medicine", false);
        var signPrescription = new Activity("Sign prescription", false);
        var rejectPrescription = new Activity("Reject prescription", false);
        var administerMedicine = new Activity("Administer medicine", false);
        var activities = new List<Activity> {prescribeMedicine, signPrescription, rejectPrescription, administerMedicine};

        var rel1 = new Relation(RelationType.CONDITION, prescribeMedicine, signPrescription);
        var rel2 = new Relation(RelationType.RESPONSE, prescribeMedicine, signPrescription);
        var rel3 = new Relation(RelationType.RESPONSE, prescribeMedicine, administerMedicine);
        var rel4 = new Relation(RelationType.CONDITION, signPrescription, administerMedicine);
        var rel5 = new Relation(RelationType.INCLUSION, signPrescription, administerMedicine);
        var rel6 = new Relation(RelationType.CONDITION, signPrescription, rejectPrescription);
        var rel7 = new Relation(RelationType.INCLUSION, signPrescription, rejectPrescription);
        var rel8 = new Relation(RelationType.RESPONSE, rejectPrescription, signPrescription);
        var rel9 = new Relation(RelationType.EXCLUSION, rejectPrescription, administerMedicine);
        var rel10 = new Relation(RelationType.EXCLUSION, administerMedicine, rejectPrescription);
        var relations = new List<Relation> { rel1, rel2, rel3, rel4, rel5, rel6, rel7, rel8, rel9, rel10 };
        var graph = new Graph(activities, relations);

        return graph;
    }

    public static Graph CreatePaperGraph()
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

        return graph;
    }

    public static Graph CreateMeetingGraph()
    {
        var proposeE1 = new Activity("Propose - E1");
        var proposeE2 = new Activity("Propose - E2");
        var acceptE1 = new Activity("Accept - E1", false, false, false);
        var acceptE2 = new Activity("Accept - E2",  false, false, false);
        var holdMeeting = new Activity("Hold meeting", true, false, false);
        var activities = new List<Activity> {proposeE1, proposeE2, acceptE1, acceptE2, holdMeeting};

        var rel1 = new Relation(RelationType.CONDITION, proposeE1, proposeE2);
        var rel2 = new Relation(RelationType.RESPONSE, proposeE1, acceptE2);
        var rel3 = new Relation(RelationType.INCLUSION, proposeE1, acceptE2);
        var rel4 = new Relation(RelationType.RESPONSE, proposeE2, acceptE1);
        var rel5 = new Relation(RelationType.INCLUSION, proposeE2, acceptE1);
        var rel6 = new Relation(RelationType.EXCLUSION, acceptE1, acceptE1);
        var rel7 = new Relation(RelationType.EXCLUSION, acceptE1, acceptE2);
        var rel8 = new Relation(RelationType.EXCLUSION, acceptE2, acceptE2);
        var rel9 = new Relation(RelationType.EXCLUSION, acceptE2, acceptE1);
        var rel10 = new Relation(RelationType.CONDITION, acceptE1, holdMeeting);
        var rel11 = new Relation(RelationType.CONDITION, acceptE2, holdMeeting);
        var rel12 = new Relation(RelationType.INCLUSION, acceptE1, holdMeeting);
        var rel13 = new Relation(RelationType.INCLUSION, acceptE2, holdMeeting);
        var relations = new List<Relation> {rel1, rel2, rel3, rel4, rel5, rel6, rel7, rel8, rel9, rel10, rel11, rel12, rel13};
        var graph = new Graph(activities, relations);

        return graph;
    }

    public static void AssertActivityStatuses(Graph graph, string activityTitle, bool enabled, bool executed, bool pending, bool included)
    {
        var activity = graph.Activities.FirstOrDefault(a => a.Title == activityTitle);

        Assert.IsNotNull(activity, $"Activity '{activityTitle}' not found in the graph.");
        Assert.AreEqual(enabled, activity.Enabled);
        Assert.AreEqual(executed, activity.Executed);
        Assert.AreEqual(pending, activity.Pending);
        Assert.AreEqual(included, activity.Included);
    }
}