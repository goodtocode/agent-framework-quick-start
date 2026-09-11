using Goodtocode.AgentFramework.Core.Application.Chats;
using Goodtocode.AgentFramework.Core.Domain.Chats;

namespace Goodtocode.AgentFramework.Tests.Integration.Chats;

[TestClass]
public sealed class ChatWriteIdempotencyTests : TestBase
{
    [TestMethod]
    public async Task CreateMyChatSession_WithSameIdempotencyKey_ReturnsExistingSession()
    {
        const string idempotencyKey = "session-idem-key";

        var first = await Sender.Send(new CreateMyChatSessionCommand
        {
            Message = "Start a new chat session",
            IdempotencyKey = idempotencyKey
        }, CancellationToken.None);

        var second = await Sender.Send(new CreateMyChatSessionCommand
        {
            Message = "Start a new chat session",
            IdempotencyKey = idempotencyKey
        }, CancellationToken.None);

        first.Id.ShouldBe(second.Id);
        (await context.ChatSessions.CountAsync(CancellationToken.None)).ShouldBe(1);
    }

    [TestMethod]
    public async Task CreateMyChatMessage_WithSameIdempotencyKey_ReturnsExistingMessageAndSkipsSecondModelTurn()
    {
        var session = ChatSessionEntity.Create(
            ownerId: rlsContext.OwnerId,
            tenantId: rlsContext.TenantId,
            actorId: Guid.NewGuid(),
            title: "Idempotent chat");
        context.ChatSessions.Add(session);
        await context.SaveChangesAsync(CancellationToken.None);

        const string idempotencyKey = "message-idem-key";

        var first = await Sender.Send(new CreateMyChatMessageCommand
        {
            ChatSessionId = session.Id,
            Message = "show my recent messages",
            IdempotencyKey = idempotencyKey
        }, CancellationToken.None);

        var second = await Sender.Send(new CreateMyChatMessageCommand
        {
            ChatSessionId = session.Id,
            Message = "show my recent messages",
            IdempotencyKey = idempotencyKey
        }, CancellationToken.None);

        first.Value.ShouldNotBeNull();
        second.Value.ShouldNotBeNull();
        first.Value!.Id.ShouldBe(second.Value!.Id);

        var persistedUserMessages = await context.ChatMessages
            .Where(x => x.ChatSessionId == session.Id && x.Role == ChatMessageRole.user)
            .ToListAsync(CancellationToken.None);

        persistedUserMessages.Count.ShouldBe(1);
        agent.RunCount.ShouldBe(0);
    }
}
