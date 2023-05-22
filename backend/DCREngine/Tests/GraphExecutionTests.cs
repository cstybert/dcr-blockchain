using NUnit.Framework;

namespace DCREngine.Tests;

public class GraphExecutionTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Execute_MedicineGraph()
    {
        var graph = TestHelper.CreateMedicineGraph();
        var prescribeMedicineTitle = "Prescribe medicine";
        var signPrescriptionTitle = "Sign prescription";
        var administerMedicineTitle = "Administer medicine";
        var rejectPrescriptionTitle = "Reject prescription";

        graph.Execute(prescribeMedicineTitle);

        TestHelper.AssertActivityStatuses(graph, prescribeMedicineTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, signPrescriptionTitle, true, false, true, true);
        TestHelper.AssertActivityStatuses(graph, rejectPrescriptionTitle, false, false, false, true);
        TestHelper.AssertActivityStatuses(graph, administerMedicineTitle, false, false, true, true);

        graph.Execute(signPrescriptionTitle);

        TestHelper.AssertActivityStatuses(graph, prescribeMedicineTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, signPrescriptionTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, rejectPrescriptionTitle, true, false, false, true);
        TestHelper.AssertActivityStatuses(graph, administerMedicineTitle, true, false, true, true);

        graph.Execute(rejectPrescriptionTitle);

        TestHelper.AssertActivityStatuses(graph, prescribeMedicineTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, signPrescriptionTitle, true, true, true, true);
        TestHelper.AssertActivityStatuses(graph, rejectPrescriptionTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, administerMedicineTitle, false, false, true, false);

        graph.Execute(signPrescriptionTitle);

        TestHelper.AssertActivityStatuses(graph, prescribeMedicineTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, signPrescriptionTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, rejectPrescriptionTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, administerMedicineTitle, true, false, true, true);

        graph.Execute(administerMedicineTitle);

        TestHelper.AssertActivityStatuses(graph, prescribeMedicineTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, signPrescriptionTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, rejectPrescriptionTitle, false, true, false, false);
        TestHelper.AssertActivityStatuses(graph, administerMedicineTitle, true, true, false, true);

        Assert.AreEqual(true, graph.Accepting);
    }

    [Test]
    public void Execute_PaperGraph()
    {
        var graph = TestHelper.CreatePaperGraph();
        var selectPapersTitle = "Select papers";
        var writeIntroductionTitle = "Write introduction";
        var writeConclusionTitle = "Write conclusion";
        var writeAbstractTitle = "Write abstract";

        graph.Execute(selectPapersTitle);

        TestHelper.AssertActivityStatuses(graph, selectPapersTitle, false, true, false, false);
        TestHelper.AssertActivityStatuses(graph, writeIntroductionTitle, true, false, true, true);
        TestHelper.AssertActivityStatuses(graph, writeConclusionTitle, true, false, true, true);
        TestHelper.AssertActivityStatuses(graph, writeAbstractTitle, true, false, true, true);

        graph.Execute(writeIntroductionTitle);

        TestHelper.AssertActivityStatuses(graph, selectPapersTitle, false, true, false, false);
        TestHelper.AssertActivityStatuses(graph, writeIntroductionTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, writeConclusionTitle, true, false, true, true);
        TestHelper.AssertActivityStatuses(graph, writeAbstractTitle, true, false, true, true);

        graph.Execute(writeAbstractTitle);

        TestHelper.AssertActivityStatuses(graph, selectPapersTitle, false, true, false, false);
        TestHelper.AssertActivityStatuses(graph, writeIntroductionTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, writeConclusionTitle, true, false, true, true);
        TestHelper.AssertActivityStatuses(graph, writeAbstractTitle, true, true, false, true);

        graph.Execute(writeConclusionTitle);

        TestHelper.AssertActivityStatuses(graph, selectPapersTitle, false, true, false, false);
        TestHelper.AssertActivityStatuses(graph, writeIntroductionTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, writeConclusionTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, writeAbstractTitle, true, true, true, true);

        graph.Execute(writeAbstractTitle);

        TestHelper.AssertActivityStatuses(graph, selectPapersTitle, false, true, false, false);
        TestHelper.AssertActivityStatuses(graph, writeIntroductionTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, writeConclusionTitle, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, writeAbstractTitle, true, true, false, true);

        Assert.AreEqual(true, graph.Accepting);
    }

    [Test]
    public void Execute_MeetingGraph()
    {
        var graph = TestHelper.CreateMeetingGraph();
        var proposeE1Title = "Propose - E1";
        var proposeE2Title = "Propose - E2";
        var acceptE1Title = "Accept - E1";
        var acceptE2Title = "Accept - E2";
        var holdMeetingTitle = "Hold meeting";

        graph.Execute(proposeE1Title);

        TestHelper.AssertActivityStatuses(graph, proposeE1Title, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, proposeE2Title, true, false, false, true);
        TestHelper.AssertActivityStatuses(graph, acceptE1Title, false, false, false, false);
        TestHelper.AssertActivityStatuses(graph, acceptE2Title, true, false, true, true);
        TestHelper.AssertActivityStatuses(graph, holdMeetingTitle, false, false, true, false);

        graph.Execute(acceptE2Title);

        TestHelper.AssertActivityStatuses(graph, proposeE1Title, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, proposeE2Title, true, false, false, true);
        TestHelper.AssertActivityStatuses(graph, acceptE1Title, false, false, false, false);
        TestHelper.AssertActivityStatuses(graph, acceptE2Title, false, true, false, false);
        TestHelper.AssertActivityStatuses(graph, holdMeetingTitle, true, false, true, true);

        graph.Execute(proposeE2Title);

        TestHelper.AssertActivityStatuses(graph, proposeE1Title, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, proposeE2Title, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, acceptE1Title, true, false, true, true);
        TestHelper.AssertActivityStatuses(graph, acceptE2Title, false, true, false, false);
        TestHelper.AssertActivityStatuses(graph, holdMeetingTitle, false, false, true, true);

        graph.Execute(acceptE1Title);

        TestHelper.AssertActivityStatuses(graph, proposeE1Title, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, proposeE2Title, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, acceptE1Title, false, true, false, false);
        TestHelper.AssertActivityStatuses(graph, acceptE2Title, false, true, false, false);
        TestHelper.AssertActivityStatuses(graph, holdMeetingTitle, true, false, true, true);

        graph.Execute(holdMeetingTitle);

        TestHelper.AssertActivityStatuses(graph, proposeE1Title, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, proposeE2Title, true, true, false, true);
        TestHelper.AssertActivityStatuses(graph, acceptE1Title, false, true, false, false);
        TestHelper.AssertActivityStatuses(graph, acceptE2Title, false, true, false, false);
        TestHelper.AssertActivityStatuses(graph, holdMeetingTitle, true, true, false, true);

        Assert.AreEqual(true, graph.Accepting);
    }
}