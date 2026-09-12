namespace Goodtocode.AgentFramework.Core.Application.Common.Idempotency;

public sealed class IdempotencyDuplicateWindowPolicy : IIdempotencyDuplicateWindowPolicy
{
    public static readonly TimeSpan NoWindow = TimeSpan.Zero;

    public TimeSpan ResolveWindow(IIdempotentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request is IIdempotencyRequestMetadata metadata
            && metadata.DuplicateWindow.HasValue
            && metadata.DuplicateWindow.Value > TimeSpan.Zero)
        {
            return metadata.DuplicateWindow.Value;
        }

        return NoWindow;
    }
}
