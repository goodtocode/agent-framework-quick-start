using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Auth;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetMyChatMessageQuery : IRequest<ChatMessageDto>, IRequiresUserContext
{
    public Guid Id { get; set; }
    public IUserContext? UserContext { get; set; }
}

public class GetChatMessageQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyChatMessageQuery, ChatMessageDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ChatMessageDto> Handle(GetMyChatMessageQuery request,
                                CancellationToken cancellationToken)
    {
        GuardAgainstEmptyUser(request?.UserContext);

        var chatMessage = await _context.ChatMessages.FindAsync([request!.Id, cancellationToken], cancellationToken: cancellationToken);
        GuardAgainstNotFound(chatMessage);
        GuardAgainstUnauthorized(chatMessage!, request.UserContext!);

        return ChatMessageDto.CreateFrom(chatMessage);
    }

    private static void GuardAgainstNotFound(ChatMessageEntity? chatMessage)
    {
        if (chatMessage == null)
            throw new CustomNotFoundException("Chat Message Not Found");
    }

    private static void GuardAgainstEmptyUser(IUserContext? userContext)
    {
        if (userContext == null || userContext.OwnerId == Guid.Empty || userContext.TenantId == Guid.Empty)
            throw new CustomValidationException(
            [
                new("UserInfo", "User information is required to retrieve a chat message")
            ]);
    }

    private static void GuardAgainstUnauthorized(ChatMessageEntity chatMessage, IUserContext userInfo)
    {
        if (chatMessage.ChatSession?.OwnerId != userInfo.OwnerId)
            throw new CustomForbiddenAccessException("ChatMessage", chatMessage.Id);
    }
}