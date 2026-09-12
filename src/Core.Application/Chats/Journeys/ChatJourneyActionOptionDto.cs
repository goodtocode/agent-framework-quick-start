namespace Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

public class ChatJourneyActionOptionDto
{
    public string Kind { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string SelectionPrompt { get; set; } = string.Empty;

    public static ChatJourneyActionOptionDto CreateFrom(ChatJourneyActionOption? actionOption)
    {
        if (actionOption is null)
        {
            return null!;
        }

        return new ChatJourneyActionOptionDto
        {
            Kind = actionOption.Kind,
            Id = actionOption.Id,
            Value = actionOption.Value,
            Label = actionOption.Label,
            SelectionPrompt = actionOption.BuildSelectionPrompt()
        };
    }
}
