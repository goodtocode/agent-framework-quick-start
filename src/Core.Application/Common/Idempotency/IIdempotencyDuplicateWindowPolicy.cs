namespace Goodtocode.AgentStart.Core.Application.Common.Idempotency;

public interface IIdempotencyDuplicateWindowPolicy
{
    TimeSpan ResolveWindow(IIdempotentRequest request);
}
