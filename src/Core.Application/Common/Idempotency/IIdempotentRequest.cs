namespace Goodtocode.AgentStart.Core.Application.Common.Idempotency;

public interface IIdempotentRequest
{
    string? IdempotencyKey { get; set; }

    string OperationKey { get; }
}
