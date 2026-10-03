namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

/// <summary>
/// Lists every Playbook catalog entry, including their CER steps, ordered by name.
/// </summary>
public class GetPlaybooksQuery : IRequest<ICollection<PlaybookDto>>
{
}

public class GetPlaybooksQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetPlaybooksQuery, ICollection<PlaybookDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ICollection<PlaybookDto>> Handle(GetPlaybooksQuery request, CancellationToken cancellationToken)
    {
        var playbooks = await _context.Playbooks
            .Include(x => x.Steps)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return playbooks.Select(PlaybookDto.CreateFrom).ToList();
    }
}
