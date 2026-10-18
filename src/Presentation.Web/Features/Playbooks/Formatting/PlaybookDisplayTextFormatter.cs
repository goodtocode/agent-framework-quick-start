using System.Text.RegularExpressions;

namespace Goodtocode.AgentFramework.Presentation.Web.Features.Playbooks.Formatting;

public static partial class PlaybookDisplayTextFormatter
{
    private static readonly Regex SentenceBoundaryRegex = new(@"(?<=[.!?;])\s+(?=[A-Z])", RegexOptions.Compiled);
    private static readonly Regex BulletBoundaryRegex = new(@"(?<=[.!?])\s+(?=-\s|\*\s)", RegexOptions.Compiled);
    private static readonly Regex WhitespaceCollapseRegex = new(@"[ \t]{2,}", RegexOptions.Compiled);

    public static string Format(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n');

        if (normalized.Contains('\n'))
        {
            return CollapseWhitespacePreservingNewlines(normalized)
                .Trim();
        }

        var withSentenceBreaks = SentenceBoundaryRegex.Replace(normalized, "\n");
        var withBulletBreaks = BulletBoundaryRegex.Replace(withSentenceBreaks, "\n");

        return CollapseWhitespacePreservingNewlines(withBulletBreaks)
            .Trim();
    }

    private static string CollapseWhitespacePreservingNewlines(string text)
    {
        var lines = text.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i] = WhitespaceCollapseRegex.Replace(lines[i], " ").Trim();
        }

        return string.Join("\n", lines);
    }
}

public static class PlaybookDisplayTextFormattingExtensions
{
    public static string FormatForPlaybookDisplay(this string? text) =>
        PlaybookDisplayTextFormatter.Format(text);
}
