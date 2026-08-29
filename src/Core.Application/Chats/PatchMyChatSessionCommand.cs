namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class PatchMyChatSessionCommand : UserScopedRequest, IRequest<CommandResult>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;

}

public class PatchChatSessionCommandHandler(IAgentFrameworkContext context) : IRequestHandler<PatchMyChatSessionCommand, CommandResult>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<CommandResult> Handle(PatchMyChatSessionCommand request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyTitle(request.Title);
        ChatGuard.GuardAgainstEmptyUserForPatch(request?.UserContext);

        var chatSession = await _context.ChatSessions.FindAsync([request!.Id, cancellationToken], cancellationToken: cancellationToken);
        if (chatSession is null)
        {
            return CommandResult.NotFound();
        }

        ChatGuard.GuardAgainstUnauthorized(chatSession, request.UserContext!);

        chatSession.Update(request.Title);

        _context.ChatSessions.Update(chatSession);
        await _context.SaveChangesAsync(cancellationToken);

        return CommandResult.Success();
    }
}
