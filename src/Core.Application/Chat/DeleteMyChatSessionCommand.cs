namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class DeleteMyChatSessionCommand : UserScopedRequest, IRequest
{
    public Guid Id { get; set; }

}

public class DeleteMyChatSessionCommandHandler(IAgentFrameworkContext context) : IRequestHandler<DeleteMyChatSessionCommand>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task Handle(DeleteMyChatSessionCommand request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyUser(request?.UserContext);

        var chatSession = _context.ChatSessions.Find(request!.Id);
        ChatGuard.GuardAgainstNotFound(chatSession);
        ChatGuard.GuardAgainstUnauthorized(chatSession!, request.UserContext!);

        _context.ChatSessions.Remove(chatSession!);
        await _context.SaveChangesAsync(cancellationToken);
    }
}