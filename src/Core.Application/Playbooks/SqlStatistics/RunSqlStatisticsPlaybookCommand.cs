using Goodtocode.AgentFramework.Core.Application.Playbooks.Persistence;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Repeatability;
using Goodtocode.Agents.Governance.Domain;
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
    public RepeatMode ReplayMode { get; init; } = RepeatMode.Rerun;
    public string? SourceExecutionId { get; init; }
}

public sealed class RunSqlStatisticsPlaybookCommandHandler(
    ISender sender,
    PlaybookRepeatCoordinator repeats,
    ISqlStatisticsClassificationRunner runner,
    SqlStatisticsStageSummarySelector summarySelector)
    : IRequestHandler<RunSqlStatisticsPlaybookCommand, PlaybookExecutionResultDto>
{
    public async Task<PlaybookExecutionResultDto> Handle(RunSqlStatisticsPlaybookCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var repeat = await repeats.PrepareAsync<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding>(
            "sql-statistics", request.UserContext, request.ReplayMode, request.SourceExecutionId, cancellationToken);
        var databaseName = repeat.SourceCollectInput ?? request.DatabaseName;

        var result = await runner.ClassifyAsync(databaseName, cancellationToken, repeat.ExecutionRequest);
        var dto = PlaybookExecutionResultDtoFactory.CreateFrom(
            databaseName, result, summarySelector, repeat.Request.Mode, repeat.SourceRecordSummary);
        dto.ExecutionId = await Persistence.PlaybookExecutionPersister.SaveAsync(sender, dto, cancellationToken);
        return dto;
    }
}
