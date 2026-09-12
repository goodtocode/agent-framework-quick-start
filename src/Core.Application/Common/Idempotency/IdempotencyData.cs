using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Goodtocode.AgentFramework.Core.Application.Common.Idempotency;

public static partial class IdempotencyData
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [GeneratedRegex("\\s+")]
    private static partial Regex MultiWhitespaceRegex();

    public static string NormalizeText(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length == 0)
        {
            return string.Empty;
        }

        return MultiWhitespaceRegex().Replace(normalized, " ").ToLowerInvariant();
    }

    public static string Sha256(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    public static string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, JsonOptions);

    public static T? Deserialize<T>(string payload)
        => string.IsNullOrWhiteSpace(payload)
            ? default
            : JsonSerializer.Deserialize<T>(payload, JsonOptions);
}
