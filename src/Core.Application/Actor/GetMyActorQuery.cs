using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Actor;
using Goodtocode.AgentFramework.Core.Domain.Auth;

namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class GetMyActorQuery : IRequest<ActorDto>, IRequiresUserContext
{
    public IUserContext? UserContext { get; set; }
}

public class GetActorByOwnerIdQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyActorQuery, ActorDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ActorDto> Handle(GetMyActorQuery request, CancellationToken cancellationToken)
    {
        var actor = await _context.Actors.Where(x => x.OwnerId == request!.UserContext!.OwnerId).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        GuardAgainstNotFound(actor);

        return ActorDto.CreateFrom(actor);
    }

    private static void GuardAgainstNotFound(ActorEntity? Actor)
    {
        if (Actor == null)
            throw new CustomNotFoundException("Actor Not Found");
    }
}