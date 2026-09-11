namespace Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

/// <summary>
/// Resolves the suggested prompts for a chat journey context. Implemented in
/// Infrastructure.AgentFramework so prompt resolution can tier the same way chat routing does:
/// Tier 1a deterministic catalog lookup, then Tier 1b embedding similarity, then Tier 2
/// agent-generated prompts.
/// </summary>
public interface IChatJourneyProvider
{
    Task<IReadOnlyList<ChatJourneySuggestedPrompt>> ResolveSuggestedPromptsAsync(
        ChatJourneyContext context,
        CancellationToken cancellationToken = default);
}
