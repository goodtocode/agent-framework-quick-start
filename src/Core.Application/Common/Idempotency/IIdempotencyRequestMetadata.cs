namespace Goodtocode.AgentStart.Core.Application.Common.Idempotency;

public interface IIdempotencyRequestMetadata
{
    string BuildRequestHash();

    Guid? ScopeId { get; }

    TimeSpan? DuplicateWindow { get; }
}
