namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class GetMyChatSessionMessagesQuery : UserScopedRequest, IRequest<ICollection<ChatMessageDto>>
{
    public Guid ChatSessionId { get; set; }
}

public class GetMyChatSessionMessagesQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyChatSessionMessagesQuery, ICollection<ChatMessageDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ICollection<ChatMessageDto>> Handle(GetMyChatSessionMessagesQuery request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyUserForQuery(request.UserContext);
        ChatGuard.GuardAgainstEmptyId(request.ChatSessionId);

        var userContext = request.UserContext;
        return await _context.ChatMessages
            .Where(x => x.ChatSessionId == request.ChatSessionId
                && x.OwnerId == userContext.OwnerId
                && x.TenantId == userContext.TenantId)
            .OrderBy(x => x.Timestamp)
            .Select(x => ChatMessageDto.CreateFrom(x))
            .ToListAsync(cancellationToken);
    }
}