namespace Goodtocode.AgentFramework.Core.Application.Common.Idempotency;

public static class IdempotencyDefaults
{
    public const string HeaderName = "Idempotency-Key";

    public static string ResolveKey(string? idempotencyKey)
        => string.IsNullOrWhiteSpace(idempotencyKey)
            ? Guid.NewGuid().ToString("N")
            : idempotencyKey.Trim();
}
