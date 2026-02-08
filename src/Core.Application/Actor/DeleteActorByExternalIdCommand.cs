using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Actor;

namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class DeleteActorByOwnerIdCommand : IRequest
{
    public Guid OwnerId { get; set; }
}

public class DeleteActorByOwnerIdCommandHandler(IAgentFrameworkContext context) : IRequestHandler<DeleteActorByOwnerIdCommand>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task Handle(DeleteActorByOwnerIdCommand request, CancellationToken cancellationToken)
    {
        var actor = await _context.Actors.Where(x => x.OwnerId == request.OwnerId).FirstOrDefaultAsync(cancellationToken);
        GuardAgainstNotFound(actor);

        _context.Actors.Remove(actor!);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void GuardAgainstNotFound(ActorEntity? Actor)
    {
        if (Actor == null)
            throw new CustomNotFoundException("Actor Not Found");
    }
}