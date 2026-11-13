using Goodtocode.AgentStart.Core.Application.Abstractions;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Intents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentStart.Tests.Integration.AgentFramework;

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
    public async Task ResolveReplyAsyncGeneralMissUsesOneAutomaticToolTurn()
    {
        var router = ServiceProvider.GetRequiredService<IChatMessageRouter>();

        var reply = await router.ResolveReplyAsync(Guid.NewGuid(), "Can you help with my saved information?", CancellationToken.None);

        Assert.AreEqual("mock-response", reply);
        Assert.AreEqual(1, agent.RunCount);
        Assert.IsNull(agent.RunOptions[0]);
    }

    [TestMethod]
    public async Task ResolveReplyAsyncRequiredToolFailurePropagatesWithoutRetry()
    {
        agent.ThrowOnForcedToolRun = true;
        var router = ServiceProvider.GetRequiredService<IChatMessageRouter>();

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
            router.ResolveReplyAsync(Guid.NewGuid(), "Explain my chat history", CancellationToken.None));
        Assert.AreEqual(1, agent.RunCount);
        Assert.IsInstanceOfType<ChatClientAgentRunOptions>(agent.RunOptions[0]);
        Assert.AreEqual(0, chatAgent.RunCount);
    }

    [TestMethod]
    public async Task ResolveReplyAsyncRequiredToolWithoutInvocationFailsWithoutRetry()
    {
        var router = ServiceProvider.GetRequiredService<IChatMessageRouter>();
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
            router.ResolveReplyAsync(Guid.NewGuid(), "Explain my chat history", CancellationToken.None));

        Assert.AreEqual(1, agent.RunCount);
        Assert.AreEqual(0, chatAgent.RunCount);
    }

    [TestMethod]
    public async Task ResolveReplyAsyncRequiredToolCancellationPropagatesWithoutRetry()
    {
        agent.CancelOnForcedToolRun = true;
        var router = ServiceProvider.GetRequiredService<IChatMessageRouter>();
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
            router.ResolveReplyAsync(Guid.NewGuid(), "Explain my chat history", CancellationToken.None));

        Assert.AreEqual(1, agent.RunCount);
        Assert.AreEqual(0, chatAgent.RunCount);
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
        Assert.AreEqual(0, agent.RunCount);
        Assert.AreEqual(1, chatAgent.RunCount);
        Assert.IsNull(chatAgent.RunOptions[0]);
        Assert.IsTrue(chatAgent.LastMessages.Any(message => message.Text.Contains("text-only assistant", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task ResolveReplyAsyncAnnouncedWebSearchIsRegeneratedWithoutFutureTense()
    {
        agent.QueuedResponseTexts.Enqueue("I will perform a web search to find the latest information on .NET.");
        chatAgent.QueuedResponseTexts.Enqueue("Web search is not available.");
        var router = ServiceProvider.GetRequiredService<IChatMessageRouter>();

        var reply = await router.ResolveReplyAsync(Guid.NewGuid(), "Tell me about the latest information on .NET", CancellationToken.None);

        Assert.AreEqual("Web search is not available.", reply);
        Assert.IsFalse(FutureTenseGuard.ContainsFutureTense(reply), $"Future tense reached the user: {reply}");
        Assert.AreEqual(1, agent.RunCount);
        Assert.AreEqual(1, chatAgent.RunCount);
        Assert.IsTrue(chatAgent.LastMessages.Any(message => message.Role == ChatRole.Assistant
            && message.Text == "I will perform a web search to find the latest information on .NET."));
    }

    [TestMethod]
    public async Task ResolveReplyAsyncPersistentFutureTenseNeverReachesTheUser()
    {
        agent.ResponseText = "I will perform a web search to find the latest information on .NET.";
        chatAgent.ResponseText = agent.ResponseText;
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