using Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;
using Goodtocode.Agents.Playbook.Execution;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Tests.Integration.Playbooks;

[TestClass]
public sealed class SqlStatisticsPlaybookTests : TestBase
{
    [TestMethod]
    public void AddSqlStatisticsPlaybookToolsDiscoversToolsByAttribute()
    {
        var resolver = ServiceProvider
            .GetRequiredService<IPlaybookStepToolResolver<string, SqlDatabaseStatisticsEvidence, SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>>();

        resolver.ResolveCollect(null).ToolName.ShouldBe("sql-statistics.deterministic.collect");
        resolver.ResolveEvaluate(null).ToolName.ShouldBe("sql-statistics.deterministic.evaluate");
        resolver.ResolveRecord(null).ToolName.ShouldBe("sql-statistics.deterministic.record");
    }

    [TestMethod]
    [DataRow(5.0, SqlDatabaseSizeClassification.Small)]
    [DataRow(20.0, SqlDatabaseSizeClassification.Small)]
    [DataRow(20.1, SqlDatabaseSizeClassification.Medium)]
    [DataRow(100.0, SqlDatabaseSizeClassification.Medium)]
    [DataRow(100.1, SqlDatabaseSizeClassification.Large)]
    public async Task ExecuteAsyncClassifiesAccordingToRubricBands(double sizeGb, SqlDatabaseSizeClassification expected)
    {
        sqlDatabaseStatisticsProvider.SizeGb = sizeGb;

        var definition = ServiceProvider.GetRequiredService<SqlStatisticsPlaybookDefinition>();
        var recorder = ServiceProvider.GetRequiredService<SqlStatisticsGovernanceActivityRecorder>();
        var executor = ServiceProvider
            .GetRequiredService<PlaybookExecutor<string, SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>>();

        var result = await executor.ExecuteAsync(
            definition,
            "Contoso",
            CancellationToken.None,
            activityRecorder: recorder);

        result.Evidence.DatabaseName.ShouldBe("Contoso");
        result.Finding.Classification.ShouldBe(expected);
        result.Materialization.Classification.ShouldBe(expected);

        recorder.Records.Count.ShouldBe(3);
        foreach (var record in recorder.Records)
        {
            string.IsNullOrWhiteSpace(record.Repeatability.PromptHash).ShouldBeFalse();
            string.IsNullOrWhiteSpace(record.Repeatability.InputHash).ShouldBeFalse();
            string.IsNullOrWhiteSpace(record.Observability.TraceId).ShouldBeFalse();
        }
    }
}
