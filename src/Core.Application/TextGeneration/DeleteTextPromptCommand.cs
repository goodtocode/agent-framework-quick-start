using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.TextGeneration;

namespace Goodtocode.AgentFramework.Core.Application.TextGeneration;

public class DeleteTextPromptCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteTextPromptCommandHandler(IAgentFrameworkContext context) : IRequestHandler<DeleteTextPromptCommand>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task Handle(DeleteTextPromptCommand request, CancellationToken cancellationToken)
    {
        var textPrompt = _context.TextPrompts.Find(request.Id);
        GuardAgainstNotFound(textPrompt);

        _context.TextPrompts.Remove(textPrompt!);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void GuardAgainstNotFound(TextPromptEntity? textPrompt)
    {
        if (textPrompt == null)
            throw new CustomNotFoundException("Chat Session Not Found");
    }
}