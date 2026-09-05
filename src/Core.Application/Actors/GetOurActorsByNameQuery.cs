namespace Goodtocode.AgentFramework.Core.Application.Actors;

public class GetOurActorsByNameQuery : UserScopedRequest, IRequest<ICollection<ActorDto>>
{
    public string Name { get; set; } = string.Empty;
}

public class GetOurActorsByNameQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetOurActorsByNameQuery, ICollection<ActorDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ICollection<ActorDto>> Handle(GetOurActorsByNameQuery request, CancellationToken cancellationToken)
    {
        var tenantId = request.UserContext.TenantId;
        var searchPattern = $"%{EscapeLikePattern(request.Name.Trim())}%";

        return await _context.Actors
            .Where(x => x.TenantId == tenantId)
            .Where(x =>
                (x.FirstName != null && EF.Functions.Like(x.FirstName, searchPattern, "\\"))
                || (x.LastName != null && EF.Functions.Like(x.LastName, searchPattern, "\\")))
            .Select(x => ActorDto.CreateFrom(x))
            .ToListAsync(cancellationToken);
    }

    private static string EscapeLikePattern(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("%", "\\%", StringComparison.Ordinal)
        .Replace("_", "\\_", StringComparison.Ordinal)
        .Replace("[", "\\[", StringComparison.Ordinal);
}