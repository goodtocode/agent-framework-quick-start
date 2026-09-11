namespace Goodtocode.AgentFramework.Presentation.Web.Features.Chats.Models;

/// <summary>
/// The customer's current position in the chat journey: suggested prompts that start or continue
/// the journey, and the follow-up action chips that match the journey context. Both submit through
/// the normal message-input path, so they stay interchangeable with a typed prompt.
/// </summary>
public sealed record ChatJourneyStepModel
{
    public string Level { get; init; } = "None";
    public IReadOnlyList<string> SuggestedPrompts { get; init; } = [];
    public IReadOnlyList<ChatSelectionOption> ActionOptions { get; init; } = [];

    public static ChatJourneyStepModel Empty { get; } = new();

    public static ChatJourneyStepModel Create(ChatJourneyStepDto? dto)
    {
        if (dto is null)
        {
            return Empty;
        }

        return new ChatJourneyStepModel
        {
            Level = dto.Level ?? "None",
            SuggestedPrompts = [.. (dto.SuggestedPrompts ?? []).Select(prompt => prompt.Prompt ?? string.Empty)
                .Where(prompt => !string.IsNullOrWhiteSpace(prompt))],
            ActionOptions = [.. (dto.ActionOptions ?? []).Select(option => new ChatSelectionOption
            {
                Kind = option.Kind ?? string.Empty,
                Id = option.Id ?? string.Empty,
                Value = option.Value ?? string.Empty,
                Label = option.Label ?? string.Empty
            })]
        };
    }
}
