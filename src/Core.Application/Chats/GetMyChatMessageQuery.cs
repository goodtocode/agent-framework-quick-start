namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class GetMyChatMessageQuery : UserScopedRequest, IRequest<ChatMessageDto?>
{
    public Guid Id { get; set; }

}

public class GetMyChatMessageQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyChatMessageQuery, ChatMessageDto?>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ChatMessageDto?> Handle(GetMyChatMessageQuery request,
                                CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyUser(request?.UserContext);

        var chatMessage = await _context.ChatMessages.FindAsync([request!.Id, cancellationToken], cancellationToken: cancellationToken);
        if (chatMessage is null)
        {
            return null;
        }

        ChatGuard.GuardAgainstUnauthorized(chatMessage, request.UserContext!);

        return ChatMessageDto.CreateFrom(chatMessage);
    }
}
