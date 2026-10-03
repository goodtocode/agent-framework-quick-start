using Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Tests.Integration.Playbooks;

[Binding]
[Scope(Tag = "taxonomyPlaybook")]
public sealed class TaxonomyPlaybookStepDefinitions : TestBase
{
    private PlaybookExecutionResult<TaxonomyEvidence, TaxonomyFinding, TaxonomyMaterialization>? _result;
    private TaxonomyGovernanceActivityRecorder? _recorder;

    [Given("the model extracts terms \"(.*)\" from the taxonomy source text")]
    public void GivenTheModelExtractsTerms(string commaSeparatedTerms)
    {
        var terms = commaSeparatedTerms.Split(',').Select(term => term.Trim());
        var termsJson = string.Join(", ", terms.Select(term => $"\"{term}\""));
        agent.QueuedResponseTexts.Enqueue($$"""{"terms": [{{termsJson}}]}""");
    }

    [Given("the model classifies the extracted terms as category \"(.*)\" with confidence (.*) and rationale \"(.*)\"")]
    public void GivenTheModelClassifiesTheExtractedTerms(string category, double confidence, string rationale)
    {
        agent.QueuedResponseTexts.Enqueue($$"""{"category": "{{category}}", "confidence": {{confidence}}, "rationale": "{{rationale}}"}""");
    }

    [Given("the model summarizes the taxonomy classification as \"(.*)\"")]
    public void GivenTheModelSummarizesTheTaxonomyClassification(string summary)
    {
        agent.QueuedResponseTexts.Enqueue($$"""{"summary": "{{summary}}"}""");
    }

    [When("I execute the taxonomy playbook for text \"(.*)\"")]
    public async Task WhenIExecuteTheTaxonomyPlaybookForText(string text)
    {
        var runner = ServiceProvider.GetRequiredService<ITaxonomyClassificationRunner>();
        _recorder = ServiceProvider.GetRequiredService<TaxonomyGovernanceActivityRecorder>();

        _result = await runner.ClassifyAsync(text, CancellationToken.None);
    }

    [Then("the taxonomy category is \"(.*)\"")]
    public void ThenTheTaxonomyCategoryIs(string category)
    {
        _result.ShouldNotBeNull();
        _result!.Finding.Category.ShouldBe(category);
    }

    [Then("(.*) governance records are produced for the taxonomy playbook")]
    public void ThenGovernanceRecordsAreProducedForTheTaxonomyPlaybook(int expectedCount)
    {
        _recorder.ShouldNotBeNull();
        _recorder!.Records.Count.ShouldBe(expectedCount);
    }
}
