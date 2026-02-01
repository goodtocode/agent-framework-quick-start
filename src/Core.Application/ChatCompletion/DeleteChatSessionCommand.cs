using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class DeleteChatSessionCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteChatSessionCommandHandler(IAgentFrameworkContext context) : IRequestHandler<DeleteChatSessionCommand>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task Handle(DeleteChatSessionCommand request, CancellationToken cancellationToken)
    {
        var chatSession = _context.ChatSessions.Find(request.Id);
        GuardAgainstNotFound(chatSession);

        _context.ChatSessions.Remove(chatSession!);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void GuardAgainstNotFound(ChatSessionEntity? chatSession)
    {
        if (chatSession == null)
            throw new CustomNotFoundException("Chat Session Not Found");
    }
}