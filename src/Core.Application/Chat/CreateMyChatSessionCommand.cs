using Goodtocode.AgentFramework.Core.Domain.Actors;
using Goodtocode.AgentFramework.Core.Domain.Chats;
using Goodtocode.AgentFramework.Core.Application.Governance;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class CreateMyChatSessionCommand : UserScopedRequest, IRequest<ChatSessionDto>
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public Guid? PersonaId { get; set; }
    public int? PersonaVersion { get; set; }

}

public class CreateMyChatSessionCommandHandler(AIAgent kernel, IAgentFrameworkContext context, ChatGovernanceGate governanceGate) : IRequestHandler<CreateMyChatSessionCommand, ChatSessionDto>
{
    private readonly AIAgent _agent = kernel;
    private readonly IAgentFrameworkContext _context = context;
    private readonly ChatGovernanceGate _governanceGate = governanceGate;

    public async Task<ChatSessionDto> Handle(CreateMyChatSessionCommand request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyMessage(request?.Message);
        ChatGuard.GuardAgainstEmptyUser(request?.UserContext);

        var actor = await _context.Actors
            .FirstOrDefaultAsync(a => a.OwnerId == request!.UserContext!.OwnerId
                && a.TenantId == request.UserContext.TenantId, cancellationToken);

        if (actor == null)
        {
            actor = ActorEntity.Create(
                ownerId: request!.UserContext.OwnerId,
                tenantId: request.UserContext.TenantId,
                firstName: request.UserContext.FirstName,
                lastName: request.UserContext.LastName,
                email: request.UserContext.Email
            );
            _context.Actors.Add(actor);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var governed = _governanceGate.Enforce(
            request.UserContext,
            Guid.Empty,
            request.Message!);

        var chatHistory = new List<ChatMessage>
        {
            new(ChatRole.System, governed.PromptContext.SystemInstruction),
            new(ChatRole.User, request!.Message!)
        };

        var agentResponse = await _agent.RunAsync(chatHistory, cancellationToken: cancellationToken);
        var response = agentResponse.Messages.LastOrDefault();

        ChatGuard.GuardAgainstNullAgentResponse(response);

        var title = request!.Title ?? $"{request!.Message![..(request.Message!.Length >= 25 ? 25 : request.Message!.Length)]}";

        var chatSession = ChatSessionEntity.Create(
            ownerId: request.UserContext.OwnerId,
            tenantId: request.UserContext.TenantId,
            actorId: actor.Id,
            title: title,
            personaId: request.PersonaId ?? Guid.Empty,
            personaVersion: request.PersonaVersion ?? 0
        );
        _context.ChatSessions.Add(chatSession);
        await _context.SaveChangesAsync(cancellationToken);

        var chatMessages = new List<ChatMessageEntity>
        {
            ChatMessageEntity.Create(
                ownerId: request.UserContext.OwnerId,
                tenantId: request.UserContext.TenantId,
                chatSessionId: chatSession.Id,
                role: ChatMessageRole.user,
                content: request!.Message!
            ),
            ChatMessageEntity.Create(
                ownerId: request.UserContext.OwnerId,
                tenantId: request.UserContext.TenantId,
                chatSessionId: chatSession.Id,
                role: ChatMessageRole.system,
                content: response!.Text
            )
        };
        _context.ChatMessages.AddRange(chatMessages);
        await _context.SaveChangesAsync(cancellationToken);

        return ChatSessionDto.CreateFrom(chatSession);
    }
}