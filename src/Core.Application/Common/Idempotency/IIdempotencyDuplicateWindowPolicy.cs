namespace Goodtocode.AgentFramework.Core.Application.Common.Idempotency;

public interface IIdempotencyDuplicateWindowPolicy
{
    TimeSpan ResolveWindow(IIdempotentRequest request);
}
