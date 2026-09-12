namespace Goodtocode.AgentFramework.Core.Application.Common.Idempotency;

public interface IIdempotencyRequestMetadata
{
    string BuildRequestHash();

    Guid? ScopeId { get; }

    TimeSpan? DuplicateWindow { get; }
}
