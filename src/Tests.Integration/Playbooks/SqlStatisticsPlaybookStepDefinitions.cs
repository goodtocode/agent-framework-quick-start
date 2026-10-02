using Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Tests.Integration.Playbooks;

[Binding]
[Scope(Tag = "sqlStatisticsPlaybook")]
public sealed class SqlStatisticsPlaybookStepDefinitions : TestBase
{
    private string _databaseName = string.Empty;
    private PlaybookExecutionResult<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>? _result;
    private SqlStatisticsGovernanceActivityRecorder? _recorder;

    [Given("the connected database reports a size of (.*) GB")]
    public void GivenTheConnectedDatabaseReportsASizeOfGb(double sizeGb)
    {
        sqlDatabaseStatisticsProvider.SizeGb = sizeGb;
    }

    [When("I execute the SQL statistics playbook for database \"(.*)\"")]
    public async Task WhenIExecuteTheSqlStatisticsPlaybookForDatabase(string databaseName)
    {
        _databaseName = databaseName;
        var definition = ServiceProvider.GetRequiredService<SqlStatisticsPlaybookDefinition>();
        var executor = ServiceProvider
            .GetRequiredService<PlaybookExecutor<string, SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>>();
        _recorder = ServiceProvider.GetRequiredService<SqlStatisticsGovernanceActivityRecorder>();

        _result = await executor.ExecuteAsync(
            definition,
            _databaseName,
            CancellationToken.None,
            activityRecorder: _recorder);
    }

    [Then("the SQL size classification is \"(.*)\"")]
    public void ThenTheSqlSizeClassificationIs(string classification)
    {
        _result.ShouldNotBeNull();
        _result!.Finding.Classification.ToString().ShouldBe(classification);
    }

    [Then("(.*) governance records are produced for the SQL statistics playbook")]
    public void ThenGovernanceRecordsAreProducedForTheSqlStatisticsPlaybook(int expectedCount)
    {
        _recorder.ShouldNotBeNull();
        _recorder!.Records.Count.ShouldBe(expectedCount);
    }
}
