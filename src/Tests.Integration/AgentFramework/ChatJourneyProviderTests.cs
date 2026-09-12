using Goodtocode.AgentFramework.Core.Application.Chats.Journeys;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Journeys;

namespace Goodtocode.AgentFramework.Tests.Integration.AgentFramework;

[TestClass]
public sealed class ChatJourneyProviderTests
{
    [TestMethod]
    public void ResolveLevelReturnsMyChatSessionsListedForMidChainEntry()
    {
        var context = new ChatJourneyContext { MyChatSessionsListed = true };

        Assert.AreEqual(ChatJourneyLevel.MyChatSessionsListed, context.ResolveLevel());
    }

    [TestMethod]
    public void ResolveLevelPrefersSelectedChatSessionOverMidChainEntry()
    {
        var context = new ChatJourneyContext
        {
            MyChatSessionsListed = true,
            SelectedChatSessionId = Guid.NewGuid()
        };

        Assert.AreEqual(ChatJourneyLevel.ChatSessionSelected, context.ResolveLevel());
    }

    [TestMethod]
    public async Task ProviderOffersMessagePromptAfterListingMyChatSessions()
    {
        var provider = CreateProvider();

        var prompts = await provider.ResolveSuggestedPromptsAsync(new ChatJourneyContext
        {
            ActorId = Guid.NewGuid(),
            MyChatSessionsListed = true
        });

        CollectionAssert.Contains(
            prompts.Select(prompt => prompt.Prompt).ToList(),
            "List my messages for this chat session");
    }

    [TestMethod]
    public async Task ProviderOffersBothMidChainEntryPointsAtTopLevel()
    {
        var provider = CreateProvider();

        var prompts = (await provider.ResolveSuggestedPromptsAsync(new ChatJourneyContext()))
            .Select(prompt => prompt.Prompt)
            .ToList();

        CollectionAssert.Contains(prompts, "List my chat sessions");
        CollectionAssert.Contains(prompts, "List my messages for this chat session");
    }

    private static RuleChatJourneyProvider CreateProvider()
        => new(new ChatJourneyCatalogFactory([new DefaultChatJourneyCatalogContributor()]));
}
