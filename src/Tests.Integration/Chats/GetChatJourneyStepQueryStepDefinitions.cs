using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Chats.Journeys;
using Goodtocode.AgentFramework.Core.Domain.Actors;
using Goodtocode.AgentFramework.Core.Domain.Chats;

namespace Goodtocode.AgentFramework.Tests.Integration.Chat;

[Binding]
[Scope(Tag = "getChatJourneyStepQuery")]
public sealed class GetChatJourneyStepQueryStepDefinitions : TestBase
{
    private Guid _activeSessionId;
    private ChatJourneyStepDto? _journeyStep;
    private string _routedAssistantResponse = string.Empty;

    [Given("I have an active chat journey session")]
    public async Task GivenIHaveAnActiveChatJourneySession()
    {
        var actor = ActorEntity.Create(
            ownerId: rlsContext.OwnerId,
            tenantId: rlsContext.TenantId,
            firstName: "Journey",
            lastName: "Owner",
            email: "journey.owner@example.test");
        context.Actors.Add(actor);
        await context.SaveChangesAsync(CancellationToken.None);

        var chatSession = ChatSessionEntity.Create(
            ownerId: rlsContext.OwnerId,
            tenantId: rlsContext.TenantId,
            actorId: actor.Id,
            title: "Journey Session");
        context.ChatSessions.Add(chatSession);
        await context.SaveChangesAsync(CancellationToken.None);

        _activeSessionId = chatSession.Id;
    }

    [Given("an actor named \"(.*)\" \"(.*)\" exists in my tenant")]
    public async Task GivenAnActorNamedExistsInMyTenant(string firstName, string lastName)
    {
        var actor = ActorEntity.Create(
            ownerId: rlsContext.OwnerId,
            tenantId: rlsContext.TenantId,
            firstName: firstName,
            lastName: lastName,
            email: $"{firstName}.{lastName}@example.test");
        context.Actors.Add(actor);
        await context.SaveChangesAsync(CancellationToken.None);
    }

    [Given("an additional chat session titled \"(.*)\" exists for me")]
    public async Task GivenAnAdditionalChatSessionTitledExistsForMe(string title)
    {
        var actor = await context.Actors
            .FirstAsync(x => x.OwnerId == rlsContext.OwnerId && x.TenantId == rlsContext.TenantId, CancellationToken.None);

        var chatSession = ChatSessionEntity.Create(
            ownerId: rlsContext.OwnerId,
            tenantId: rlsContext.TenantId,
            actorId: actor.Id,
            title: title);
        context.ChatSessions.Add(chatSession);
        await context.SaveChangesAsync(CancellationToken.None);
    }

    [When("I route the chat message \"(.*)\"")]
    public async Task WhenIRouteTheChatMessage(string message)
    {
        var router = ServiceProvider.GetRequiredService<IChatMessageRouter>();
        _routedAssistantResponse = await router.ResolveReplyAsync(_activeSessionId, message, CancellationToken.None);
    }

    [When("I persist the routed assistant response for the active session")]
    public async Task WhenIPersistTheRoutedAssistantResponseForTheActiveSession()
    {
        var assistantMessage = ChatMessageEntity.Create(
            ownerId: rlsContext.OwnerId,
            tenantId: rlsContext.TenantId,
            chatSessionId: _activeSessionId,
            role: ChatMessageRole.assistant,
            content: _routedAssistantResponse);

        context.ChatMessages.Add(assistantMessage);
        await context.SaveChangesAsync(CancellationToken.None);
    }

    [When("I get the chat journey step for the active session")]
    public async Task WhenIGetTheChatJourneyStepForTheActiveSession()
    {
        _journeyStep = await Sender.Send(new GetChatJourneyStepQuery
        {
            ChatSessionId = _activeSessionId
        }, CancellationToken.None);
    }

    [Then("the journey level is \"(.*)\"")]
    public void ThenTheJourneyLevelIs(string expectedLevel)
    {
        _journeyStep.ShouldNotBeNull();
        _journeyStep!.Level.ShouldBe(expectedLevel);
    }

    [Then("the suggested prompt \"(.*)\" is returned")]
    public void ThenTheSuggestedPromptIsReturned(string expectedPrompt)
    {
        _journeyStep.ShouldNotBeNull();
        _journeyStep!.SuggestedPrompts.Any(x => x.Prompt == expectedPrompt).ShouldBeTrue();
    }

    [Then("no journey action options are returned")]
    public void ThenNoJourneyActionOptionsAreReturned()
    {
        _journeyStep.ShouldNotBeNull();
        _journeyStep!.ActionOptions.Count.ShouldBe(0);
    }

    [Then("a journey action option with kind \"(.*)\" is returned")]
    public void ThenAJourneyActionOptionWithKindIsReturned(string kind)
    {
        _journeyStep.ShouldNotBeNull();
        _journeyStep!.ActionOptions.Any(x => x.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Then("the router did not run the AI agent")]
    public void ThenTheRouterDidNotRunTheAIAgent()
    {
        agent.RunCount.ShouldBe(0);
    }
}
