using Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;
using Goodtocode.Agents.Playbook.Execution;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Tests.Integration.Playbooks;

[TestClass]
public sealed class TaxonomyPlaybookTests : TestBase
{
    [TestMethod]
    public void AddTaxonomyPlaybookToolsDiscoversToolsByAttribute()
    {
        var resolver = ServiceProvider
            .GetRequiredService<IPlaybookStepToolResolver<string, TaxonomyEvidence, TaxonomyEvidence, TaxonomyFinding, TaxonomyFinding, TaxonomyMaterialization>>();

        resolver.ResolveCollect(null).ToolName.ShouldBe("taxonomy.agentic.collect");
        resolver.ResolveEvaluate(null).ToolName.ShouldBe("taxonomy.agentic.evaluate");
        resolver.ResolveRecord(null).ToolName.ShouldBe("taxonomy.agentic.record");
    }

    [TestMethod]
    public async Task ClassifyAsyncRunsAllThreeAgenticStagesAndProducesGovernanceRecords()
    {
        agent.QueuedResponseTexts.Enqueue("""{"terms": ["budget", "forecast"]}""");
        agent.QueuedResponseTexts.Enqueue("""{"category": "Finance", "confidence": 0.9, "rationale": "Discusses budgeting."}""");
        agent.QueuedResponseTexts.Enqueue("""{"summary": "Classified as Finance with high confidence."}""");

        var runner = ServiceProvider.GetRequiredService<ITaxonomyClassificationRunner>();
        var recorder = ServiceProvider.GetRequiredService<TaxonomyGovernanceActivityRecorder>();

        var result = await runner.ClassifyAsync("Quarterly budget forecast", CancellationToken.None);

        result.Evidence.ExtractedTerms.Count.ShouldBe(2);
        result.Evidence.ExtractedTerms[0].ShouldBe("budget");
        result.Evidence.ExtractedTerms[1].ShouldBe("forecast");
        result.Finding.Category.ShouldBe("Finance");
        result.Finding.Confidence.ShouldBe(0.9);
        result.Materialization.Summary.ShouldBe("Classified as Finance with high confidence.");

        recorder.Records.Count.ShouldBe(3);
        foreach (var record in recorder.Records)
        {
            record.Repeatability.DeterministicReplaySupported.ShouldBeFalse();
            string.IsNullOrWhiteSpace(record.Observability.TraceId).ShouldBeFalse();
        }
    }

    [TestMethod]
    public async Task EvaluateThrowsWhenModelReturnsAnUnrecognizedCategory()
    {
        agent.QueuedResponseTexts.Enqueue("""{"terms": ["budget"]}""");
        agent.QueuedResponseTexts.Enqueue("""{"category": "NotARealCategory", "confidence": 0.5, "rationale": "n/a"}""");

        var runner = ServiceProvider.GetRequiredService<ITaxonomyClassificationRunner>();

        InvalidOperationException? caught = null;
        try
        {
            await runner.ClassifyAsync("Quarterly budget forecast", CancellationToken.None);
        }
        catch (InvalidOperationException exception)
        {
            caught = exception;
        }

        caught.ShouldNotBeNull();
    }
}
