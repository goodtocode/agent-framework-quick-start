using System.Globalization;
using System.Text;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework;

public static class MarkdownTableFormatter
{
    public static string Format(
        IReadOnlyList<string> headers,
        IEnumerable<IReadOnlyList<string?>> rows)
    {
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(rows);

        var markdown = new StringBuilder();
        markdown.AppendLine(CultureInfo.InvariantCulture,
            $"| {string.Join(" | ", headers.Select(EscapeCell))} |");
        markdown.AppendLine(CultureInfo.InvariantCulture,
            $"| {string.Join(" | ", headers.Select(_ => "---"))} |");

        var rowNumber = 0;
        foreach (var row in rows)
        {
            if (row.Count != headers.Count)
            {
                throw new ArgumentException(
                    $"Row {rowNumber} contains {row.Count} cells, but the table has {headers.Count} headers.",
                    nameof(rows));
            }

            markdown.AppendLine(CultureInfo.InvariantCulture,
                $"| {string.Join(" | ", row.Select(EscapeCell))} |");
            rowNumber++;
        }

        return markdown.ToString();
    }

    public static string EscapeCell(string? value) => (value ?? string.Empty)
        .Replace("|", "\\|", StringComparison.Ordinal)
        .Replace("\r", " ", StringComparison.Ordinal)
        .Replace("\n", " ", StringComparison.Ordinal);
}
