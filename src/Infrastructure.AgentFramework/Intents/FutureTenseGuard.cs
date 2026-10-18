using System.Text;
using System.Text.RegularExpressions;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// Validates model-generated replies against the no-future-tense rule: a reply may only report what is
/// done or state that the request cannot be completed. Promises of later work never reach the user
/// because an action not performed within the reply will never be performed.
/// </summary>
public static partial class FutureTenseGuard
{
    public const string FallbackReply = "The request was not completed. No result was produced.";

    [GeneratedRegex(
        @"\bI\s*(?:will|shall|['’]ll)\b"
        + @"|\b(?:I|we)\s*(?:am|are|['’]m|['’]re)\s+(?:going\s+to|about\s+to|gonna)\b"
        + @"|\bwe\s*(?:will|shall|['’]ll)\b"
        + @"|\bI\s+(?:plan|intend|aim|propose)\s+to\b"
        + @"|\b(?:let\s+me|allow\s+me|permit\s+me)\b"
        + @"|\bgive\s+me\s+(?:a\s+)?(?:moment|second|minute|sec)\b"
        + @"|\b(?:one|just\s+a)\s+(?:moment|second|minute)\b"
        + @"|\b(?:please\s+wait|hold\s+on|stand\s+by|stay\s+tuned|bear\s+with\s+me)\b"
        + @"|\bnext,?\s+I\b"
        + @"|\b(?:in|within)\s+a\s+(?:moment|few\s+moments|few\s+seconds)\b"
        + @"|\bwill\s+(?:now\s+)?(?:search|look|fetch|query|check|find|retrieve|run|get|proceed|begin|start)\b"
        + @"|\bgoing\s+to\s+(?:search|look|fetch|query|check|find|retrieve|run|get)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex FutureTensePattern();

    [GeneratedRegex(@"(?<=[.!?])\s+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SentenceBoundary();

    public static bool ContainsFutureTense(string? reply)
        => !string.IsNullOrWhiteSpace(reply) && FutureTensePattern().IsMatch(reply);

    /// <summary>
    /// Returns the reply unchanged when compliant. Otherwise removes every violating sentence (table rows
    /// and selection chips are data and are kept as-is) and returns <see cref="FallbackReply"/> when nothing remains.
    /// </summary>
    public static string Enforce(string? reply)
    {
        if (string.IsNullOrWhiteSpace(reply))
        {
            return reply ?? string.Empty;
        }

        if (!ContainsFutureTense(reply))
        {
            return reply;
        }

        var result = new StringBuilder();
        foreach (var line in reply.Split('\n'))
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith('|') || trimmed.StartsWith("[selection|", StringComparison.Ordinal))
            {
                result.Append(line.TrimEnd('\r')).Append('\n');
                continue;
            }

            var kept = SentenceBoundary()
                .Split(line.TrimEnd('\r'))
                .Where(sentence => !FutureTensePattern().IsMatch(sentence));
            var rebuilt = string.Join(' ', kept).TrimEnd();
            if (rebuilt.Length > 0 || line.Trim().Length == 0)
            {
                result.Append(rebuilt).Append('\n');
            }
        }

        var cleaned = result.ToString().Trim();
        return cleaned.Length == 0 ? FallbackReply : cleaned;
    }
}
