using Goodtocode.AgentFramework.Core.Application.Playbooks.Persistence;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;

/// <summary>
/// API-facing command for running the SQL Statistics Classification example playbook end to end
/// and projecting the result into the shared <see cref="PlaybookExecutionResultDto"/> shape used
/// by every example playbook's "run" endpoint. <see cref="ReplayMode"/> selects the repeatability
/// behavior: <c>Rerun</c> (default) executes fresh against <see cref="DatabaseName"/>; <c>Recall</c>
/// and <c>Replay</c> ignore <see cref="DatabaseName"/> and instead rehydrate the prior execution
/// identified by <see cref="SourceExecutionId"/> (or the user's latest execution, if omitted).
/// </summary>
public sealed class RunSqlStatisticsPlaybookCommand : UserScopedRequest, IRequest<PlaybookExecutionResultDto>
{
    public required string DatabaseName { get; init; }
    public PlaybookReplayMode ReplayMode { get; init; } = PlaybookReplayMode.Rerun;
    public string? SourceExecutionId { get; init; }
}

public sealed class RunSqlStatisticsPlaybookCommandHandler(
    ISender sender,
    IAgentFrameworkContext context,
    ISqlStatisticsClassificationRunner runner,
    SqlStatisticsStageSummarySelector summarySelector)
    : IRequestHandler<RunSqlStatisticsPlaybookCommand, PlaybookExecutionResultDto>
{
    public async Task<PlaybookExecutionResultDto> Handle(RunSqlStatisticsPlaybookCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string databaseName;
        PlaybookReplayContext<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding>? replayContext = null;

        if (request.ReplayMode == PlaybookReplayMode.Rerun)
        {
            databaseName = request.DatabaseName;
        }
        else
        {
            var source = await PlaybookReplaySourceResolver.ResolveAsync(
                context, "sql-statistics", request.UserContext.OwnerId, request.UserContext.TenantId,
                request.SourceExecutionId, cancellationToken);

            databaseName = source.CollectInput;
            var priorEvidence = System.Text.Json.JsonSerializer.Deserialize<SqlDatabaseStatisticsEvidence>(source.EvidenceJson!)!;
            var priorFinding = System.Text.Json.JsonSerializer.Deserialize<SqlDatabaseSizeFinding>(source.FindingJson!)!;
            replayContext = new PlaybookReplayContext<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding>(
                request.ReplayMode, source.Id.ToString(), priorEvidence, priorFinding);
        }

        var result = await runner.ClassifyAsync(databaseName, cancellationToken, replayContext);
        var dto = PlaybookExecutionResultDtoFactory.CreateFrom(databaseName, result, summarySelector);
        dto.ExecutionId = await Persistence.PlaybookExecutionPersister.SaveAsync(sender, dto, cancellationToken);
        return dto;
    }
}
