namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class DeleteMyChatSessionCommand : UserScopedRequest, IRequest<CommandResult>
{
    public Guid Id { get; set; }

}

public class DeleteMyChatSessionCommandHandler(IAgentFrameworkContext context) : IRequestHandler<DeleteMyChatSessionCommand, CommandResult>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<CommandResult> Handle(DeleteMyChatSessionCommand request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyUser(request?.UserContext);

        var chatSession = await _context.ChatSessions.FindAsync([request!.Id, cancellationToken], cancellationToken: cancellationToken);
        if (chatSession is null)
        {
            return CommandResult.NotFound();
        }

        ChatGuard.GuardAgainstUnauthorized(chatSession, request.UserContext!);

        _context.ChatSessions.Remove(chatSession);
        await _context.SaveChangesAsync(cancellationToken);

        return CommandResult.Success();
    }
}
