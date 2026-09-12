using System.Text.RegularExpressions;

namespace Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

/// <summary>
/// Pure, deterministic implementation of <see cref="IChatSelectionTokenParser"/>. Has no AI or
/// persistence dependency, so it is registered as a plain application service.
/// </summary>
public sealed partial class ChatSelectionTokenParser : IChatSelectionTokenParser
{
    [GeneratedRegex(@"\[selection\|(?<kind>[^|\]]+)\|(?<id>[^|\]]+)\|(?<value>[^|\]]*)\|(?<label>[^\]]*)\]")]
    private static partial Regex SelectionTokenRegex();

    public IReadOnlyList<ChatJourneyActionOption> Parse(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        var options = new List<ChatJourneyActionOption>();
        foreach (Match match in SelectionTokenRegex().Matches(content))
        {
            var kind = match.Groups["kind"].Value.Trim();
            var id = match.Groups["id"].Value.Trim();
            if (string.IsNullOrWhiteSpace(kind) || string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            var value = match.Groups["value"].Value.Trim();
            var label = match.Groups["label"].Value.Trim();
            options.Add(new ChatJourneyActionOption(
                kind,
                id,
                value,
                string.IsNullOrWhiteSpace(label) ? $"{kind}:{id}" : label));
        }

        return options;
    }

    public string Strip(string? content)
        => string.IsNullOrWhiteSpace(content)
            ? string.Empty
            : SelectionTokenRegex().Replace(content, string.Empty).Trim();
}
