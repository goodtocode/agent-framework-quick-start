using System.Text;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// Performs lightweight, deterministic token normalization and intent-rule matching.
/// </summary>
public static class IntentTokenMatcher
{
    private static readonly IReadOnlyDictionary<string, string> Aliases =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["actors"] = "actor",
            ["called"] = "name",
            ["chats"] = "chat",
            ["conversations"] = "conversation",
            ["looked"] = "lookup",
            ["named"] = "name",
            ["messages"] = "message",
            ["sessions"] = "session"
        };

    public static IReadOnlySet<string> Normalize(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var builder = new StringBuilder(text.Length);
        foreach (var character in text.ToLowerInvariant())
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : ' ');
        }

        return builder
            .ToString()
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(token => Aliases.TryGetValue(token, out var alias) ? alias : token)
            .ToHashSet(StringComparer.Ordinal);
    }

    public static bool IsMatch(IntentTokenRule rule, IReadOnlySet<string> tokens)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(tokens);

        return rule.AllOf.All(tokens.Contains)
            && rule.AnyOfGroups.All(group => group.Any(tokens.Contains))
            && !(rule.NoneOf?.Any(tokens.Contains) ?? false);
    }

    public static int Specificity(IntentTokenRule rule) =>
        rule.AllOf.Count + rule.AnyOfGroups.Count;
}
