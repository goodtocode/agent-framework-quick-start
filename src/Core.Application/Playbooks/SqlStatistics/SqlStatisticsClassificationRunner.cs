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
        PlaybookReplayContext<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding>? replayContext = null);
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
        PlaybookReplayContext<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding>? replayContext = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        return replayContext is null
            ? executor.ExecuteAsync(definition, databaseName, cancellationToken, activityRecorder: recorder)
            : executor.ExecuteAsync(definition, databaseName, replayContext, cancellationToken, activityRecorder: recorder);
    }
}
