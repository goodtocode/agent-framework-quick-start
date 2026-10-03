using Goodtocode.AgentFramework.Core.Application.Playbooks;
using Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;
using Goodtocode.AgentFramework.Core.Domain.Playbooks;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Tests.Integration.Playbooks;

/// <summary>
/// Verifies the repeatability governance pillar end to end through the real Run*PlaybookCommand
/// pipeline: a Rerun persists the typed evidence/finding needed for later Recall/Replay requests,
/// Recall re-renders the prior finding without any new Collect/Evaluate work, and Replay reuses
/// the prior evidence while re-running Evaluate and Record. Covers both the deterministic SQL
/// Statistics playbook (the package's own <c>PlaybookExecutor</c> replay overload) and the
/// agentic Taxonomy playbook (the repo-owned <see cref="Goodtocode.AgentFramework.Infrastructure.AgentFramework.Execution.PlaybookWorkflowGraphExecutor{TCollectInput,TEvidence,TFinding,TMaterialization}"/>),
/// since each uses a different replay mechanism.
/// </summary>
[TestClass]
public sealed class PlaybookReplayTests : TestBase
{
    private async Task SeedSqlStatisticsPlaybookAsync()
    {
        await Sender.Send(new CreatePlaybookCommand
        {
            Key = "sql-statistics",
            Name = "SQL Statistics Classification",
            Description = "Deterministic CER workflow for test purposes.",
            WorkflowType = "Deterministic",
            Version = "v1",
            Collect = new PlaybookStepInput
            {
                Name = "Collect database statistics",
                Description = "Runs a deterministic T-SQL statistics query.",
                ActionFormat = PlaybookActionFormat.SqlQuery,
                ActionDefinition = "SELECT 1;"
            },
            Evaluate = new PlaybookStepInput
            {
                Name = "Classify database size",
                Description = "Classifies the collected SizeGb against named size bands.",
                ActionFormat = PlaybookActionFormat.Rubric,
                ActionDefinition = "Small: 0-20 GB; Medium: 20-100 GB; Large: 100+ GB."
            },
            Record = new PlaybookStepInput
            {
                Name = "Record classification",
                Description = "Projects the classification into a display summary.",
                ActionFormat = PlaybookActionFormat.Template,
                ActionDefinition = "{DatabaseName} is {Classification} at {SizeGb} GB."
            }
        }, CancellationToken.None);
    }

    private async Task SeedTaxonomyPlaybookAsync()
    {
        await Sender.Send(new CreatePlaybookCommand
        {
            Key = "taxonomy",
            Name = "Taxonomy Extraction and Classification",
            Description = "Agentic CER workflow for test purposes.",
            WorkflowType = "Agentic",
            Version = "v1",
            Collect = new PlaybookStepInput
            {
                Name = "Extract taxonomy terms",
                Description = "Asks the model to extract terms.",
                ActionFormat = PlaybookActionFormat.Prompt,
                ActionDefinition = "Extract terms from {input}."
            },
            Evaluate = new PlaybookStepInput
            {
                Name = "Classify into taxonomy category",
                Description = "Asks the model to classify the terms.",
                ActionFormat = PlaybookActionFormat.Prompt,
                ActionDefinition = "Classify {terms}."
            },
            Record = new PlaybookStepInput
            {
                Name = "Record classification",
                Description = "Projects the finding into a summary.",
                ActionFormat = PlaybookActionFormat.Template,
                ActionDefinition = "Classified as {Category}."
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task RerunPersistsEvidenceAndFindingForLaterReplay()
    {
        await SeedSqlStatisticsPlaybookAsync();
        sqlDatabaseStatisticsProvider.SizeGb = 50;

        var dto = await Sender.Send(new RunSqlStatisticsPlaybookCommand { DatabaseName = "Contoso" }, CancellationToken.None);

        dto.ReplayMode.ShouldBe(nameof(PlaybookReplayMode.Rerun));
        dto.ExecutionId.ShouldNotBeEmpty();
        string.IsNullOrWhiteSpace(dto.EvidenceJson).ShouldBeFalse();
        string.IsNullOrWhiteSpace(dto.FindingJson).ShouldBeFalse();

        var persisted = await context.PlaybookExecutions.FindAsync(dto.ExecutionId);
        persisted.ShouldNotBeNull();
        string.IsNullOrWhiteSpace(persisted!.EvidenceJson).ShouldBeFalse();
        string.IsNullOrWhiteSpace(persisted.FindingJson).ShouldBeFalse();
    }

    [TestMethod]
    public async Task RecallReusesPriorEvidenceAndFindingWithoutRecollecting()
    {
        await SeedSqlStatisticsPlaybookAsync();
        sqlDatabaseStatisticsProvider.SizeGb = 50;

        var original = await Sender.Send(new RunSqlStatisticsPlaybookCommand { DatabaseName = "Contoso" }, CancellationToken.None);

        // Change the live database size; Recall must still report the original Medium finding
        // since it never re-collects or re-evaluates.
        sqlDatabaseStatisticsProvider.SizeGb = 500;

        var recalled = await Sender.Send(new RunSqlStatisticsPlaybookCommand
        {
            DatabaseName = "ignored-for-recall",
            ReplayMode = PlaybookReplayMode.Recall,
            SourceExecutionId = original.ExecutionId.ToString()
        }, CancellationToken.None);

        recalled.ReplayMode.ShouldBe(nameof(PlaybookReplayMode.Recall));
        recalled.SourceExecutionId.ShouldBe(original.ExecutionId.ToString());
        recalled.CollectInput.ShouldBe("Contoso");
        recalled.EvaluateSummary.ShouldBe(original.EvaluateSummary);
    }

    [TestMethod]
    public async Task ReplayReusesPriorEvidenceButReEvaluates()
    {
        await SeedSqlStatisticsPlaybookAsync();
        sqlDatabaseStatisticsProvider.SizeGb = 50;

        var original = await Sender.Send(new RunSqlStatisticsPlaybookCommand { DatabaseName = "Contoso" }, CancellationToken.None);

        // Change the live database size; Replay must still classify against the original 50 GB
        // evidence (Medium) since it skips Collect, even though the live size is now Large.
        sqlDatabaseStatisticsProvider.SizeGb = 500;

        var replayed = await Sender.Send(new RunSqlStatisticsPlaybookCommand
        {
            DatabaseName = "ignored-for-replay",
            ReplayMode = PlaybookReplayMode.Replay,
            SourceExecutionId = original.ExecutionId.ToString()
        }, CancellationToken.None);

        replayed.ReplayMode.ShouldBe(nameof(PlaybookReplayMode.Replay));
        replayed.SourceExecutionId.ShouldBe(original.ExecutionId.ToString());
        replayed.CollectInput.ShouldBe("Contoso");
        replayed.EvaluateSummary.ShouldBe(original.EvaluateSummary);
    }

    [TestMethod]
    public async Task TaxonomyRecallSkipsCollectAndEvaluateAgentCalls()
    {
        await SeedTaxonomyPlaybookAsync();

        agent.QueuedResponseTexts.Enqueue("""{"terms": ["budget", "forecast"]}""");
        agent.QueuedResponseTexts.Enqueue("""{"category": "Finance", "confidence": 0.9, "rationale": "Discusses budgeting."}""");
        agent.QueuedResponseTexts.Enqueue("""{"summary": "Classified as Finance with high confidence."}""");

        var original = await Sender.Send(new RunTaxonomyPlaybookCommand { Text = "Quarterly budget forecast" }, CancellationToken.None);

        // Only the Record stage's agent call is queued; Recall must not invoke Collect or
        // Evaluate, so no extra queued responses are needed for those stages.
        agent.QueuedResponseTexts.Enqueue("""{"summary": "Recalled: Classified as Finance with high confidence."}""");

        var recalled = await Sender.Send(new RunTaxonomyPlaybookCommand
        {
            Text = "ignored-for-recall",
            ReplayMode = PlaybookReplayMode.Recall,
            SourceExecutionId = original.ExecutionId.ToString()
        }, CancellationToken.None);

        recalled.ReplayMode.ShouldBe(nameof(PlaybookReplayMode.Recall));
        recalled.EvaluateSummary.ShouldBe(original.EvaluateSummary);
        agent.QueuedResponseTexts.Count.ShouldBe(0);
    }
}
