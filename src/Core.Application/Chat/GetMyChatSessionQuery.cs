namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class GetMyChatSessionQuery : UserScopedRequest, IRequest<ChatSessionDto>
{
    public Guid Id { get; set; }

}

public class GetMyChatSessionQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyChatSessionQuery, ChatSessionDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ChatSessionDto> Handle(GetMyChatSessionQuery request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyUserForQuery(request?.UserContext);
        ChatGuard.GuardAgainstEmptyId(request?.Id);

        var chatSession = await _context.ChatSessions
            .FirstOrDefaultAsync(x => x.Id == request!.Id && x.OwnerId == request.UserContext.OwnerId && x.TenantId == request.UserContext.TenantId, cancellationToken: cancellationToken);
        ChatGuard.GuardAgainstNotFound(chatSession);
        ChatGuard.GuardAgainstUnauthorized(chatSession!, request!.UserContext!);

        return ChatSessionDto.CreateFrom(chatSession);
    }
}