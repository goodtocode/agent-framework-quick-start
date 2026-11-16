using Goodtocode.AgentStart.Core.Application.Chats;

namespace Goodtocode.AgentStart.Core.Application.Actors;

public class GetOurActorChatSessionQuery : UserScopedRequest, IRequest<ChatSessionDto?>
{
    public Guid ActorId { get; set; }
    public Guid ChatSessionId { get; set; }

}

public class GetOurActorChatSessionQueryHandler(IApplicationDbContext context) : IRequestHandler<GetOurActorChatSessionQuery, ChatSessionDto?>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<ChatSessionDto?> Handle(GetOurActorChatSessionQuery request, CancellationToken cancellationToken)
    {
        var returnData = await _context.ChatSessions
            .FirstOrDefaultAsync(x => x.Id == request.ChatSessionId && x.ActorId == request.ActorId && x.TenantId == request.UserContext.TenantId, cancellationToken: cancellationToken);

        return returnData is null ? null : ChatSessionDto.CreateFrom(returnData);
    }
}
