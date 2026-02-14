using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Domain.Actor;
using Goodtocode.AgentFramework.Core.Domain.Auth;

namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class SaveMyActorCommand : IRequest<ActorDto>, IRequiresUserContext
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public IUserContext? UserContext { get; set; }
}

public class SaveActorCommandHandler(IAgentFrameworkContext context) : IRequestHandler<SaveMyActorCommand, ActorDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ActorDto> Handle(SaveMyActorCommand request, CancellationToken cancellationToken)
    {
        GuardAgainstInvalidUserContext(request?.UserContext);

        var actor = await _context.Actors.Where(x => x.OwnerId == request!.UserContext!.OwnerId && x.TenantId == request.UserContext.TenantId).FirstOrDefaultAsync(cancellationToken);
        if (actor is not null)
        {
            actor.Update(request?.FirstName, request?.LastName ?? actor.LastName, request?.Email);
            _context.Actors.Update(actor!);
        }
        else
        {
            actor = ActorEntity.Create(Guid.NewGuid(), request?.FirstName, request?.LastName, request?.Email, request!.UserContext!.OwnerId, request.UserContext.TenantId);
            _context.Actors.Add(actor);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ActorDto.CreateFrom(actor);
    }

    private static void GuardAgainstInvalidUserContext(IUserContext? userContext)
    {
        if (userContext is null)
            throw new CustomValidationException(
            [
                new("UserContext", "UserContext is required to save an actor")
            ]);

        if (userContext.OwnerId == Guid.Empty)
            throw new CustomValidationException(
            [
                new("OwnerId", "A valid OwnerId is required in UserContext to save an actor")
            ]);
    }

    private static void GuardAgainstEmptyTenantId(Guid? tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new CustomValidationException(
            [
                new("TenantId", "A TenantId is required to link an actor with an account")
            ]);
    }
}