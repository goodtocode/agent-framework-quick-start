using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Tests.Integration.AgentFramework;

[TestClass]
public sealed class ChatMessageRouterTests : TestBase
{
    [TestMethod]
    public async Task ResolveReplyAsyncDeterministicIntentSkipsAgentRuns()
    {
        var router = ServiceProvider.GetRequiredService<IChatMessageRouter>();

        var reply = await router.ResolveReplyAsync(Guid.NewGuid(), "List my chat sessions", CancellationToken.None);

        Assert.AreEqual("You have no chat sessions yet.", reply);
        Assert.AreEqual(0, agent.RunCount);
    }

    [TestMethod]
    public async Task ResolveReplyAsyncForcedToolReplyWithoutInvocationFallsThroughToOpenAgentTurn()
    {
        var router = ServiceProvider.GetRequiredService<IChatMessageRouter>();

        var reply = await router.ResolveReplyAsync(Guid.NewGuid(), "Can you help with my saved information?", CancellationToken.None);

        Assert.AreEqual("mock-response", reply);
        Assert.AreEqual(2, agent.RunCount);
        Assert.IsInstanceOfType<ChatClientAgentRunOptions>(agent.RunOptions[0]);
        Assert.IsNull(agent.RunOptions[1]);
    }

    [TestMethod]
    public async Task ResolveReplyAsyncForcedToolFailureFallsThroughToOpenAgentTurn()
    {
        agent.ThrowOnForcedToolRun = true;
        var router = ServiceProvider.GetRequiredService<IChatMessageRouter>();

        var reply = await router.ResolveReplyAsync(Guid.NewGuid(), "Can you help with my saved information?", CancellationToken.None);

        Assert.AreEqual("mock-response", reply);
        Assert.AreEqual(2, agent.RunCount);
        Assert.IsInstanceOfType<ChatClientAgentRunOptions>(agent.RunOptions[0]);
        Assert.IsNull(agent.RunOptions[1]);
    }

    [TestMethod]
    public async Task ResolveReplyAsyncDirectModeSkipsTheFirstTwoTiers()
    {
        var router = ServiceProvider.GetRequiredService<IChatMessageRouter>();

        var reply = await router.ResolveReplyAsync(
            Guid.NewGuid(),
            "List my chat sessions",
            CancellationToken.None,
            ChatRoutingMode.Direct);

        Assert.AreEqual("mock-response", reply);
        Assert.AreEqual(1, agent.RunCount);
        Assert.IsNull(agent.RunOptions[0]);
    }

    [TestMethod]
    public async Task ResolveReplyAsyncAnnouncedWebSearchIsRegeneratedWithoutFutureTense()
    {
        agent.QueuedResponseTexts.Enqueue("forced-tool-no-call");
        agent.QueuedResponseTexts.Enqueue("I will perform a web search to find the latest information on .NET.");
        agent.QueuedResponseTexts.Enqueue("Web search is not available.");
        var router = ServiceProvider.GetRequiredService<IChatMessageRouter>();

        var reply = await router.ResolveReplyAsync(Guid.NewGuid(), "Tell me about the latest information on .NET", CancellationToken.None);

        Assert.AreEqual("Web search is not available.", reply);
        Assert.IsFalse(FutureTenseGuard.ContainsFutureTense(reply), $"Future tense reached the user: {reply}");
    }

    [TestMethod]
    public async Task ResolveReplyAsyncPersistentFutureTenseNeverReachesTheUser()
    {
        agent.ResponseText = "I will perform a web search to find the latest information on .NET.";
        var router = ServiceProvider.GetRequiredService<IChatMessageRouter>();

        var reply = await router.ResolveReplyAsync(Guid.NewGuid(), "Tell me about the latest information on .NET", CancellationToken.None);

        Assert.AreEqual(FutureTenseGuard.FallbackReply, reply);
        Assert.IsFalse(FutureTenseGuard.ContainsFutureTense(reply), $"Future tense reached the user: {reply}");
    }

    [TestMethod]
    public async Task ResolveReplyAsyncEveryInferenceCarriesTheNoFutureTensePreamble()
    {
        var router = ServiceProvider.GetRequiredService<IChatMessageRouter>();

        await router.ResolveReplyAsync(Guid.NewGuid(), "Tell me about the latest information on .NET", CancellationToken.None);

        Assert.IsTrue(agent.LastMessages.Any(message =>
            message.Role == ChatRole.System
            && message.Text == ToolRoutingInstructions.NoFutureTensePreamble));
    }
}