using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Auth;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetMyChatSessionQuery : IRequest<ChatSessionDto>, IRequiresUserContext
{
    public Guid Id { get; set; }
    public IUserContext? UserContext { get; set; }
}

public class GetMyChatSessionQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyChatSessionQuery, ChatSessionDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ChatSessionDto> Handle(GetMyChatSessionQuery request, CancellationToken cancellationToken)
    {
        GuardAgainstEmptyUser(request?.UserContext);

        var chatSession = await _context.ChatSessions.FindAsync([request!.Id], cancellationToken: cancellationToken);
        GuardAgainstNotFound(chatSession);
        GuardAgainstUnauthorized(chatSession!, request.UserContext!);

        return ChatSessionDto.CreateFrom(chatSession);
    }

    private static void GuardAgainstNotFound(ChatSessionEntity? entity)
    {
        if (entity is null)
            throw new CustomNotFoundException("Chat Session Not Found");
    }

    private static void GuardAgainstEmptyUser(IUserContext? userContext)
    {
        if (userContext == null || userContext.OwnerId == Guid.Empty || userContext.TenantId == Guid.Empty)
            throw new CustomValidationException(
            [
                new("UserInfo", "User information is required to retrieve a chat session")
            ]);
    }

    private static void GuardAgainstUnauthorized(ChatSessionEntity chatSession, IUserContext userInfo)
    {
        if (chatSession.OwnerId != userInfo.OwnerId)
            throw new CustomForbiddenAccessException("ChatSession", chatSession.Id);
    }
}