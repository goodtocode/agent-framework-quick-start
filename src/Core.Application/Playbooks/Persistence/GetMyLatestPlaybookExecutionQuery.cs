namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Persistence;

/// <summary>
/// Fetches the current user's most recently completed execution of one Playbook, so a Playbook
/// page can show "the summary of the latest run" (including its final Record-stage result) as
/// soon as the page loads, without requiring the user to re-run the Playbook first.
/// </summary>
public class GetMyLatestPlaybookExecutionQuery : UserScopedRequest, IRequest<PlaybookExecutionResultDto?>
{
    public required string PlaybookKey { get; init; }
}

public class GetMyLatestPlaybookExecutionQueryHandler(IAgentFrameworkContext context)
    : IRequestHandler<GetMyLatestPlaybookExecutionQuery, PlaybookExecutionResultDto?>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<PlaybookExecutionResultDto?> Handle(GetMyLatestPlaybookExecutionQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await _context.PlaybookExecutions
            .Where(x => x.PlaybookKey == request.PlaybookKey
                        && x.OwnerId == request.UserContext.OwnerId
                        && x.TenantId == request.UserContext.TenantId)
            .OrderByDescending(x => x.CompletedUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : PlaybookExecutionResultDtoFactory.CreateFrom(entity);
    }
}
