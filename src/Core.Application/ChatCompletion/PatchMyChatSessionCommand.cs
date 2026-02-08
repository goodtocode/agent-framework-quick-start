using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Auth;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class PatchMyChatSessionCommand : IRequest, IRequiresUserContext
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public IUserContext? UserContext { get; set; }
}

public class PatchChatSessionCommandHandler(IAgentFrameworkContext context) : IRequestHandler<PatchMyChatSessionCommand>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task Handle(PatchMyChatSessionCommand request, CancellationToken cancellationToken)
    {
        GuardAgainstEmptyTitle(request.Title);
        GuardAgainstEmptyUser(request?.UserContext);

        var chatSession = _context.ChatSessions.Find(request!.Id);
        GuardAgainstNotFound(chatSession);
        GuardAgainstUnauthorized(chatSession!, request.UserContext!);

        chatSession!.Update(request.Title);

        _context.ChatSessions.Update(chatSession);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void GuardAgainstNotFound(ChatSessionEntity? chatSession)
    {
        if (chatSession == null)
            throw new CustomNotFoundException("Chat Session Not Found");
    }

    private static void GuardAgainstEmptyTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new CustomValidationException(
                [
                    new("Title", "Title cannot be empty")
                ]);
    }

    private static void GuardAgainstEmptyUser(IUserContext? userContext)
    {
        if (userContext == null || userContext.OwnerId == Guid.Empty || userContext.TenantId == Guid.Empty)
            throw new CustomValidationException(
            [
                new("UserContext", "User information is required to update a chat session")
            ]);
    }

    private static void GuardAgainstUnauthorized(ChatSessionEntity chatSession, IUserContext userContext)
    {
        if (chatSession.OwnerId != userContext.OwnerId)
            throw new CustomForbiddenAccessException("ChatSession", chatSession.Id);
    }
}