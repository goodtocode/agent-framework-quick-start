namespace Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

public class ChatJourneySuggestedPromptDto
{
    public string Prompt { get; set; } = string.Empty;

    public static ChatJourneySuggestedPromptDto CreateFrom(ChatJourneySuggestedPrompt? suggestedPrompt)
    {
        if (suggestedPrompt is null)
        {
            return null!;
        }

        return new ChatJourneySuggestedPromptDto
        {
            Prompt = suggestedPrompt.Prompt
        };
    }
}
