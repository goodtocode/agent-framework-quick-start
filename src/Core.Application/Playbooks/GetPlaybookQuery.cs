namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

/// <summary>
/// Retrieves one Playbook catalog entry, including its three CER steps, by either its Id or its
/// canonical Key (e.g. "sql-statistics").
/// </summary>
public class GetPlaybookQuery : IRequest<PlaybookDto?>
{
    public Guid? Id { get; init; }
    public string? Key { get; init; }
}

public class GetPlaybookQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetPlaybookQuery, PlaybookDto?>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<PlaybookDto?> Handle(GetPlaybookQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var playbook = await _context.Playbooks
            .Include(x => x.Steps)
            .Where(x => (request.Id != null && x.Id == request.Id) || (request.Key != null && x.Key == request.Key))
            .FirstOrDefaultAsync(cancellationToken);

        return playbook is null ? null : PlaybookDto.CreateFrom(playbook);
    }
}
