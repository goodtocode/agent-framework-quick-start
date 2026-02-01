using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class PatchChatSessionCommand : IRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class PatchChatSessionCommandHandler(IAgentFrameworkContext context) : IRequestHandler<PatchChatSessionCommand>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task Handle(PatchChatSessionCommand request, CancellationToken cancellationToken)
    {

        var chatSession = _context.ChatSessions.Find(request.Id);
        GuardAgainstNotFound(chatSession);
        GuardAgainstEmptyTitle(request.Title);

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
}