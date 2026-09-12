namespace Goodtocode.AgentFramework.Core.Application.Common.Idempotency;

public abstract class IdempotentUserScopedRequest : UserScopedRequest, IIdempotentRequest
{
    public string? IdempotencyKey { get; set; }

    public virtual string OperationKey => GetType().FullName ?? GetType().Name;
}
