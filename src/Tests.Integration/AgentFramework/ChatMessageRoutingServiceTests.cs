using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Goodtocode.AgentFramework.Tests.Integration.AgentFramework;

[TestClass]
public sealed class ChatMessageRoutingServiceTests : TestBase
{
    [TestMethod]
    public async Task ResolveReplyAsyncDeterministicIntentSkipsAgentRuns()
    {
        var router = ServiceProvider.GetRequiredService<IChatMessageRoutingService>();

        var reply = await router.ResolveReplyAsync(Guid.NewGuid(), "List my chat sessions", CancellationToken.None);

        Assert.AreEqual("You have no chat sessions yet.", reply);
        Assert.AreEqual(0, agent.RunCount);
    }

    [TestMethod]
    public async Task ResolveReplyAsyncAmbiguousMessageReturnsForcedToolReply()
    {
        var router = ServiceProvider.GetRequiredService<IChatMessageRoutingService>();

        var reply = await router.ResolveReplyAsync(Guid.NewGuid(), "Can you help with my saved information?", CancellationToken.None);

        Assert.AreEqual("mock-response", reply);
        Assert.AreEqual(1, agent.RunCount);
        Assert.IsInstanceOfType<ChatClientAgentRunOptions>(agent.RunOptions[0]);
    }

    [TestMethod]
    public async Task ResolveReplyAsyncForcedToolFailureFallsThroughToOpenAgentTurn()
    {
        agent.ThrowOnForcedToolRun = true;
        var router = ServiceProvider.GetRequiredService<IChatMessageRoutingService>();

        var reply = await router.ResolveReplyAsync(Guid.NewGuid(), "Can you help with my saved information?", CancellationToken.None);

        Assert.AreEqual("mock-response", reply);
        Assert.AreEqual(2, agent.RunCount);
        Assert.IsInstanceOfType<ChatClientAgentRunOptions>(agent.RunOptions[0]);
        Assert.IsNull(agent.RunOptions[1]);
    }

    [TestMethod]
    public async Task ResolveReplyAsyncDirectModeSkipsTheFirstTwoTiers()
    {
        var router = ServiceProvider.GetRequiredService<IChatMessageRoutingService>();

        var reply = await router.ResolveReplyAsync(
            Guid.NewGuid(),
            "List my chat sessions",
            CancellationToken.None,
            ChatRoutingMode.Direct);

        Assert.AreEqual("mock-response", reply);
        Assert.AreEqual(1, agent.RunCount);
        Assert.IsNull(agent.RunOptions[0]);
    }
}