using Goodtocode.AgentFramework.Core.Application.Chat;
using Goodtocode.AgentFramework.Core.Domain.Chat;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Tests.Integration.Governance;

[TestClass]
public class ChatGovernanceInvocationTests : TestBase
{
    [TestMethod]
    public async Task CreateChatSessionUsesGovernedSystemInstruction()
    {
        await Sender.Send(new CreateMyChatSessionCommand
        {
            Message = "Start a governed chat session"
        }, CancellationToken.None);

        (agent.LastMessages.Count > 1).ShouldBeTrue();
        agent.LastMessages[0].Role.ShouldBe(ChatRole.System);
        string.IsNullOrWhiteSpace(agent.LastMessages[0].Text).ShouldBeFalse();
    }

    [TestMethod]
    public async Task CreateChatMessageUsesGovernedSystemInstruction()
    {
        var session = ChatSessionEntity.Create(
            ownerId: rlsContext.OwnerId,
            tenantId: rlsContext.TenantId,
            actorId: Guid.NewGuid(),
            title: "Governed chat");
        context.ChatSessions.Add(session);
        await context.SaveChangesAsync(CancellationToken.None);

        await Sender.Send(new CreateMyChatMessageCommand
        {
            ChatSessionId = session.Id,
            Message = "Continue the governed chat"
        }, CancellationToken.None);

        (agent.LastMessages.Count > 1).ShouldBeTrue();
        agent.LastMessages[0].Role.ShouldBe(ChatRole.System);
        string.IsNullOrWhiteSpace(agent.LastMessages[0].Text).ShouldBeFalse();
    }
}