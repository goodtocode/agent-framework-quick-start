using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Audio;

namespace Goodtocode.AgentFramework.Core.Application.Audio;

public class DeleteTextAudioCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteTextAudioCommandHandler(IAgentFrameworkContext context) : IRequestHandler<DeleteTextAudioCommand>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task Handle(DeleteTextAudioCommand request, CancellationToken cancellationToken)
    {
        var textAudio = _context.TextAudio.Find(request.Id);
        GuardAgainstNotFound(textAudio);

        _context.TextAudio.Remove(textAudio!);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void GuardAgainstNotFound(TextAudioEntity? textAudio)
    {
        if (textAudio == null)
            throw new CustomNotFoundException("Text Audio Not Found");
    }
}