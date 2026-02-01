using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetChatSessionQuery : IRequest<ChatSessionDto>
{
    public Guid Id { get; set; }
}

public class GetChatSessionQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetChatSessionQuery, ChatSessionDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ChatSessionDto> Handle(GetChatSessionQuery request,
                                CancellationToken cancellationToken)
    {
        var chatSession = await _context.ChatSessions.FindAsync([request.Id, cancellationToken], cancellationToken: cancellationToken);
        GuardAgainstNotFound(chatSession);

        return ChatSessionDto.CreateFrom(chatSession);
    }

    private static void GuardAgainstNotFound(ChatSessionEntity? chatSession)
    {
        if (chatSession == null)
            throw new CustomNotFoundException("Chat Session Not Found");
    }
}