using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Actor;

namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class DeleteActorCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteAuthorCommandHandler(IAgentFrameworkContext context) : IRequestHandler<DeleteActorCommand>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task Handle(DeleteActorCommand request, CancellationToken cancellationToken)
    {
        var Actor = _context.Actors.Find(request.Id);
        GuardAgainstNotFound(Actor);

        _context.Actors.Remove(Actor!);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void GuardAgainstNotFound(ActorEntity? Actor)
    {
        if (Actor == null)
            throw new CustomNotFoundException("Actor Not Found");
    }
}