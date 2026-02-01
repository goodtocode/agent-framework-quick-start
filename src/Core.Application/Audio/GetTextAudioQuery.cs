using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Audio;

namespace Goodtocode.AgentFramework.Core.Application.Audio;

public class GetTextAudioQuery : IRequest<TextAudioDto>
{
    public Guid Id { get; set; }
}

public class GetTextAudioQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetTextAudioQuery, TextAudioDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<TextAudioDto> Handle(GetTextAudioQuery request,
                                CancellationToken cancellationToken)
    {
        var textAudio = await _context.TextAudio.FindAsync([request.Id, cancellationToken], cancellationToken: cancellationToken);
        GuardAgainstNotFound(textAudio);

        return TextAudioDto.CreateFrom(textAudio);
    }

    private static void GuardAgainstNotFound(TextAudioEntity? textAudio)
    {
        if (textAudio == null)
            throw new CustomNotFoundException("Text Audio Not Found");
    }
}