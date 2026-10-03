namespace Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;

/// <summary>
/// API-facing command for running the SQL Statistics Classification example playbook end to end
/// and projecting the result into the shared <see cref="PlaybookExecutionResultDto"/> shape used
/// by every example playbook's "run" endpoint.
/// </summary>
public sealed class RunSqlStatisticsPlaybookCommand : IRequest<PlaybookExecutionResultDto>
{
    public required string DatabaseName { get; init; }
}

public sealed class RunSqlStatisticsPlaybookCommandHandler(
    ISqlStatisticsClassificationRunner runner,
    SqlStatisticsStageSummarySelector summarySelector)
    : IRequestHandler<RunSqlStatisticsPlaybookCommand, PlaybookExecutionResultDto>
{
    public async Task<PlaybookExecutionResultDto> Handle(RunSqlStatisticsPlaybookCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await runner.ClassifyAsync(request.DatabaseName, cancellationToken);
        return PlaybookExecutionResultDtoFactory.CreateFrom(request.DatabaseName, result, summarySelector);
    }
}
