namespace Goodtocode.AgentFramework.Core.Application.Actor;

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
        var nameTokens = request.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var normalizedInput = request.Name.Trim();

        return await _context.Actors
            .Where(x => x.TenantId == tenantId)
            .Where(x =>
                nameTokens.Any(token =>
                    EF.Functions.Like(x.FirstName, $"%{token}%") ||
                    EF.Functions.Like(x.LastName, $"%{token}%"))
                || EF.Functions.Like((x.FirstName + " " + x.LastName).Trim(), $"%{normalizedInput}%")
                || EF.Functions.Like((x.LastName + " " + x.FirstName).Trim(), $"%{normalizedInput}%")
                || nameTokens.Any(token =>
                    EF.Functions.Like(x.FirstName, $"{token}%") ||
                    EF.Functions.Like(x.FirstName, $"%{token}") ||
                    EF.Functions.Like(x.LastName, $"{token}%") ||
                    EF.Functions.Like(x.LastName, $"%{token}")))
            .Select(x => ActorDto.CreateFrom(x))
            .ToListAsync(cancellationToken);
    }
}