namespace Goodtocode.AgentFramework.Core.Application.Common.Idempotency;

public interface IIdempotentRequest
{
    string? IdempotencyKey { get; set; }

    string OperationKey { get; }
}
