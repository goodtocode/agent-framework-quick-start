using Goodtocode.AgentFramework.Core.Application.Playbooks;
using Goodtocode.Agents.Playbook.Execution;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Tests.Integration.Playbooks;

[TestClass]
public sealed class DocumentReviewPlaybookTests : TestBase
{
    [TestMethod]
    public void AddDocumentReviewPlaybookToolsDiscoversToolsByAttribute()
    {
        var resolver = ServiceProvider
            .GetRequiredService<IPlaybookStepToolResolver<ReviewRequest, ReviewEvidence, ReviewEvidence, ReviewFinding, ReviewFinding, ReviewRecord>>();

        resolver.ResolveCollect(null).ToolName.ShouldBe("document-review.deterministic.collect");
        resolver.ResolveEvaluate(null).ToolName.ShouldBe("document-review.deterministic.evaluate");
        resolver.ResolveRecord(null).ToolName.ShouldBe("document-review.deterministic.record");
    }

    [TestMethod]
    public async Task ExecuteAsyncRunsAllThreeStagesAndProducesGovernanceRecords()
    {
        var definition = ServiceProvider.GetRequiredService<DocumentReviewPlaybookDefinition>();
        var recorder = ServiceProvider.GetRequiredService<PlaybookGovernanceActivityRecorder>();
        var executor = ServiceProvider.GetRequiredService<PlaybookExecutor<ReviewRequest, ReviewEvidence, ReviewFinding, ReviewRecord>>();

        var result = await executor.ExecuteAsync(
            definition,
            new ReviewRequest("Document content"),
            CancellationToken.None,
            activityRecorder: recorder);

        result.Evidence.Document.ShouldBe("Document content");
        result.Finding.Approved.ShouldBeTrue();
        result.Materialization.Status.ShouldBe("Approved");

        recorder.Records.Count.ShouldBe(3);
        foreach (var record in recorder.Records)
        {
            string.IsNullOrWhiteSpace(record.Repeatability.PromptHash).ShouldBeFalse();
            string.IsNullOrWhiteSpace(record.Repeatability.InputHash).ShouldBeFalse();
            string.IsNullOrWhiteSpace(record.Observability.TraceId).ShouldBeFalse();
        }
    }
}
