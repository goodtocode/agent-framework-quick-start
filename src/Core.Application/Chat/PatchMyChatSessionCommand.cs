namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class PatchMyChatSessionCommand : UserScopedRequest, IRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;

}

public class PatchChatSessionCommandHandler(IAgentFrameworkContext context) : IRequestHandler<PatchMyChatSessionCommand>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task Handle(PatchMyChatSessionCommand request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyTitle(request.Title);
        ChatGuard.GuardAgainstEmptyUserForPatch(request?.UserContext);

        var chatSession = _context.ChatSessions.Find(request!.Id);
        ChatGuard.GuardAgainstNotFound(chatSession);
        ChatGuard.GuardAgainstUnauthorized(chatSession!, request.UserContext!);

        chatSession!.Update(request.Title);

        _context.ChatSessions.Update(chatSession);
        await _context.SaveChangesAsync(cancellationToken);
    }
}