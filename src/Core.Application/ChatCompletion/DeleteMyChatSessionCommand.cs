using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Auth;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class DeleteMyChatSessionCommand : IRequest, IRequiresUserContext
{
    public Guid Id { get; set; }
    public IUserContext? UserContext { get; set; }
}

public class DeleteChatSessionCommandHandler(IAgentFrameworkContext context) : IRequestHandler<DeleteMyChatSessionCommand>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task Handle(DeleteMyChatSessionCommand request, CancellationToken cancellationToken)
    {
        GuardAgainstEmptyUser(request?.UserContext);

        var chatSession = _context.ChatSessions.Find(request!.Id);
        GuardAgainstNotFound(chatSession);
        GuardAgainstUnauthorized(chatSession!, request.UserContext!);

        _context.ChatSessions.Remove(chatSession!);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void GuardAgainstNotFound(ChatSessionEntity? chatSession)
    {
        if (chatSession == null)
            throw new CustomNotFoundException("Chat Session Not Found");
    }

    private static void GuardAgainstEmptyUser(IUserContext? userContext)
    {
        if (userContext == null || userContext.OwnerId == Guid.Empty || userContext.TenantId == Guid.Empty)
            throw new CustomValidationException(
            [
                new("UserContext", "User information is required to delete a chat session")
            ]);
    }

    private static void GuardAgainstUnauthorized(ChatSessionEntity chatSession, IUserContext userContext)
    {
        if (chatSession.OwnerId != userContext.OwnerId)
            throw new CustomForbiddenAccessException("ChatSession", chatSession.Id);
    }
}