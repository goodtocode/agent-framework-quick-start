using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Image;

namespace Goodtocode.AgentFramework.Core.Application.Image;

public class GetTextImageQuery : IRequest<TextImageDto>
{
    public Guid Id { get; set; }
}

public class GetTextImageQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetTextImageQuery, TextImageDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<TextImageDto> Handle(GetTextImageQuery request,
                                CancellationToken cancellationToken)
    {
        var textImage = await _context.TextImages.FindAsync([request.Id, cancellationToken], cancellationToken: cancellationToken);
        GuardAgainstNotFound(textImage);

        return TextImageDto.CreateFrom(textImage);
    }

    private static void GuardAgainstNotFound(TextImageEntity? textImage)
    {
        if (textImage == null)
            throw new CustomNotFoundException("Text Image Not Found");
    }
}