using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;

/// <summary>
/// Framework-agnostic entry point for running the SQL Statistics Classification example playbook.
/// Unlike the Taxonomy and Essay runners, this one needs no Microsoft Agent Framework graph - the
/// workflow is fully deterministic - so it is implemented directly in Core.Application using the
/// package's own <see cref="PlaybookExecutor{TCollectInput,TEvidence,TFinding,TMaterialization}"/>,
/// while still exposing the same <c>ClassifyAsync(string, CancellationToken)</c> shape as
/// <see cref="Taxonomy.ITaxonomyClassificationRunner"/> and <see cref="Essay.IEssayEvaluationRunner"/>.
/// </summary>
public interface ISqlStatisticsClassificationRunner
{
    Task<PlaybookExecutionResult<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>> ClassifyAsync(
        string databaseName,
        CancellationToken cancellationToken,
        PlaybookExecutionRequest<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding>? request = null);
}

public sealed class SqlStatisticsClassificationRunner(
    SqlStatisticsPlaybookDefinition definition,
    SqlStatisticsGovernanceActivityRecorder recorder,
    PlaybookExecutor<string, SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization> executor)
    : ISqlStatisticsClassificationRunner
{
    public Task<PlaybookExecutionResult<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>> ClassifyAsync(
        string databaseName,
        CancellationToken cancellationToken,
        PlaybookExecutionRequest<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding>? request = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        return executor.ExecuteAsync(
            definition, databaseName, request ?? PlaybookExecutionRequest<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding>.Default,
            cancellationToken, activityRecorder: recorder);
    }
}
