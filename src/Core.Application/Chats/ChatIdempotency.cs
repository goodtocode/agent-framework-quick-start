using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Goodtocode.AgentFramework.Core.Application.Chats;

public static partial class ChatIdempotency
{
    public const string HeaderName = "Idempotency-Key";
    public const string CreateSessionOperation = "CreateMyChatSession";
    public const string CreateMessageOperation = "CreateMyChatMessage";
    public static readonly TimeSpan DuplicateWindow = TimeSpan.FromSeconds(5);

    [GeneratedRegex("\\s+")]
    private static partial Regex MultiWhitespaceRegex();

    public static string NormalizeMessage(string? message)
    {
        var value = message?.Trim() ?? string.Empty;
        if (value.Length == 0)
        {
            return string.Empty;
        }

        return MultiWhitespaceRegex().Replace(value, " ").ToLowerInvariant();
    }

    public static string Sha256(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
