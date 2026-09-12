namespace Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

public class ChatJourneyStepDto
{
    public string Level { get; set; } = ChatJourneyLevel.None.ToString();
    public ICollection<ChatJourneySuggestedPromptDto> SuggestedPrompts { get; set; } = [];
    public ICollection<ChatJourneyActionOptionDto> ActionOptions { get; set; } = [];

    public static ChatJourneyStepDto CreateFrom(ChatJourneyStep? step)
    {
        if (step is null)
        {
            return null!;
        }

        return new ChatJourneyStepDto
        {
            Level = step.Level.ToString(),
            SuggestedPrompts = [.. step.SuggestedPrompts.Select(ChatJourneySuggestedPromptDto.CreateFrom)],
            ActionOptions = [.. step.ActionOptions.Select(ChatJourneyActionOptionDto.CreateFrom)]
        };
    }
}
