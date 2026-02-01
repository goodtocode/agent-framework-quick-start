using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Image;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class DeleteTextImageCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteTextImageCommandHandler(IAgentFrameworkContext context) : IRequestHandler<DeleteTextImageCommand>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task Handle(DeleteTextImageCommand request, CancellationToken cancellationToken)
    {
        var textImage = await _context.TextImages.FindAsync([request.Id], cancellationToken);
        GuardAgainstNotFound(textImage);

        _context.TextImages.Remove(textImage!);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void GuardAgainstNotFound(TextImageEntity? textImage)
    {
        if (textImage == null)
            throw new CustomNotFoundException("Text Image Not Found");
    }
}
