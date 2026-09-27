using Goodtocode.AgentFramework.Core.Application.Playbooks;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Tests.Integration.Playbooks;

[Binding]
[Scope(Tag = "documentReviewPlaybook")]
public sealed class DocumentReviewPlaybookStepDefinitions : TestBase
{
    private ReviewRequest _request = new(string.Empty);
    private PlaybookExecutionResult<ReviewEvidence, ReviewFinding, ReviewRecord>? _result;
    private PlaybookGovernanceActivityRecorder? _recorder;

    [Given("I have a document review request with content \"(.*)\"")]
    public void GivenIHaveADocumentReviewRequestWithContent(string document)
    {
        _request = new ReviewRequest(document);
    }

    [When("I execute the document review playbook")]
    public async Task WhenIExecuteTheDocumentReviewPlaybook()
    {
        var definition = ServiceProvider.GetRequiredService<DocumentReviewPlaybookDefinition>();
        var executor = ServiceProvider.GetRequiredService<PlaybookExecutor<ReviewRequest, ReviewEvidence, ReviewFinding, ReviewRecord>>();
        _recorder = ServiceProvider.GetRequiredService<PlaybookGovernanceActivityRecorder>();

        _result = await executor.ExecuteAsync(definition, _request, CancellationToken.None, activityRecorder: _recorder);
    }

    [Then("the review status is \"(.*)\"")]
    public void ThenTheReviewStatusIs(string status)
    {
        _result.ShouldNotBeNull();
        _result!.Materialization.Status.ShouldBe(status);
    }

    [Then("the review is approved is \"(.*)\"")]
    public void ThenTheReviewIsApprovedIs(string approved)
    {
        _result.ShouldNotBeNull();
        _result!.Finding.Approved.ShouldBe(bool.Parse(approved));
    }

    [Then("(.*) governance records are produced")]
    public void ThenGovernanceRecordsAreProduced(int expectedCount)
    {
        _recorder.ShouldNotBeNull();
        _recorder!.Records.Count.ShouldBe(expectedCount);
    }
}
