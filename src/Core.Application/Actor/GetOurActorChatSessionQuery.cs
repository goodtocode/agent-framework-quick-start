using Goodtocode.AgentFramework.Core.Application.Chat;

namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class GetOurActorChatSessionQuery : UserScopedRequest, IRequest<ChatSessionDto>
{
    public Guid ActorId { get; set; }
    public Guid ChatSessionId { get; set; }

}

public class GetOurActorChatSessionQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetOurActorChatSessionQuery, ChatSessionDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ChatSessionDto> Handle(GetOurActorChatSessionQuery request, CancellationToken cancellationToken)
    {
        var returnData = await _context.ChatSessions
            .FirstOrDefaultAsync(x => x.Id == request.ChatSessionId && x.ActorId == request.ActorId && x.TenantId == request.UserContext.TenantId, cancellationToken: cancellationToken);
        ActorGuard.GuardAgainstNotFound(returnData);

        return ChatSessionDto.CreateFrom(returnData);
    }
}