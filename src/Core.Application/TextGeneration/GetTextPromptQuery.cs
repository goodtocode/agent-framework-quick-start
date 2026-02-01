using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.TextGeneration;

namespace Goodtocode.AgentFramework.Core.Application.TextGeneration;

public class GetTextPromptQuery : IRequest<TextPromptDto>
{
    public Guid Id { get; set; }
}

public class GetTextPromptQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetTextPromptQuery, TextPromptDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<TextPromptDto> Handle(GetTextPromptQuery request,
                                CancellationToken cancellationToken)
    {
        var textPrompt = await _context.TextPrompts.FindAsync([request.Id, cancellationToken], cancellationToken: cancellationToken);
        GuardAgainstNotFound(textPrompt);

        return TextPromptDto.CreateFrom(textPrompt);
    }

    private static void GuardAgainstNotFound(TextPromptEntity? textPrompt)
    {
        if (textPrompt == null)
            throw new CustomNotFoundException("Chat Session Not Found");
    }
}