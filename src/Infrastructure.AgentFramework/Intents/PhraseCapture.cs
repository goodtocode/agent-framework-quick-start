namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// A readable, regex-free replacement for parameterized "select X {value}"/"search the web for
/// {query}" patterns. Declares a literal prefix phrase to look for (case-insensitive, matched
/// anywhere in the message) plus the shape of the value that follows it (<see cref="CaptureKind"/>),
/// and exposes that value under <paramref name="CaptureName"/> in <see cref="IntentMatch.Captures"/>.
/// </summary>
/// <param name="Prefix">Literal phrase preceding the captured value, e.g. "select actor ".</param>
/// <param name="CaptureName">Key under which the captured value is exposed in <see cref="IntentMatch.Captures"/>.</param>
/// <param name="Kind">Shape of the value following <paramref name="Prefix"/>.</param>
public sealed record PhraseCapture(string Prefix, string CaptureName, CaptureKind Kind)
{
    /// <summary>
    /// Attempts to find <see cref="Prefix"/> in <paramref name="message"/> and extract the value that
    /// follows it according to <see cref="Kind"/>. Returns <see langword="false"/> if the prefix isn't
    /// present or no valid value of the expected shape follows it.
    /// </summary>
    public bool TryMatch(string message, out string value)
    {
        value = string.Empty;

        var searchStart = 0;
        while (true)
        {
            var prefixIndex = message.IndexOf(Prefix, searchStart, StringComparison.OrdinalIgnoreCase);
            if (prefixIndex < 0)
            {
                return false;
            }

            // Mirror the original regex's \b word-boundary: don't match "select" inside "reselect".
            var precededByWordChar = prefixIndex > 0 && char.IsLetterOrDigit(message[prefixIndex - 1]);
            if (!precededByWordChar)
            {
                var remainder = message[(prefixIndex + Prefix.Length)..];
                return Kind switch
                {
                    CaptureKind.GuidDFormat => TryTakeGuid(remainder, out value),
                    CaptureKind.Word => TryTakeWord(remainder, out value),
                    CaptureKind.Rest => TryTakeRest(remainder, out value),
                    _ => false
                };
            }

            searchStart = prefixIndex + 1;
        }
    }

    private static bool TryTakeGuid(string remainder, out string value)
    {
        // "D" format: 8-4-4-4-12 hex digits, e.g. 3fa85f64-5717-4562-b3fc-2c963f66afa6 (36 chars).
        const int guidLength = 36;
        if (remainder.Length >= guidLength
            && Guid.TryParseExact(remainder[..guidLength], "D", out var parsed)
            && !HasTrailingWordChar(remainder, guidLength))
        {
            value = parsed.ToString("D");
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool TryTakeWord(string remainder, out string value)
    {
        var end = 0;
        while (end < remainder.Length && IsWordChar(remainder[end]))
        {
            end++;
        }

        value = remainder[..end];
        return value.Length > 0;
    }

    private static bool TryTakeRest(string remainder, out string value)
    {
        value = remainder.Trim();
        return value.Length > 0;
    }

    private static bool IsWordChar(char c) =>
        char.IsLetterOrDigit(c) || c is '_' or '.' or ':' or '-';

    // Mirrors the trailing \b in the original regex: the captured value must not be immediately
    // followed by another word character (e.g. a GUID directly abutting more hex digits shouldn't match).
    private static bool HasTrailingWordChar(string remainder, int matchedLength) =>
        matchedLength < remainder.Length && char.IsLetterOrDigit(remainder[matchedLength]);
}
