using Goodtocode.AgentFramework.Core.Application.Actor;
using Goodtocode.AgentFramework.Core.Application.Chat;

namespace Goodtocode.AgentFramework.Tests.Integration.Application;

[TestClass]
public class NotFoundSemanticsTests : TestBase
{
    [TestMethod]
    public async Task GetOurActorQueryReturnsNullWhenActorMissing()
    {
        var result = await Sender.Send(new GetOurActorQuery { ActorId = Guid.NewGuid() }, CancellationToken.None);

        result.ShouldBeNull();
    }

    [TestMethod]
    public async Task GetMyChatSessionQueryReturnsNullWhenSessionMissing()
    {
        var result = await Sender.Send(new GetMyChatSessionQuery { Id = Guid.NewGuid() }, CancellationToken.None);

        result.ShouldBeNull();
    }

    [TestMethod]
    public async Task DeleteMyChatSessionCommandReturnsNotFoundWhenSessionMissing()
    {
        var result = await Sender.Send(new DeleteMyChatSessionCommand { Id = Guid.NewGuid() }, CancellationToken.None);

        result.IsNotFound.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
    }

    [TestMethod]
    public async Task PatchMyChatSessionCommandReturnsNotFoundWhenSessionMissing()
    {
        var result = await Sender.Send(new PatchMyChatSessionCommand
        {
            Id = Guid.NewGuid(),
            Title = "Updated"
        }, CancellationToken.None);

        result.IsNotFound.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
    }

    [TestMethod]
    public async Task CreateMyChatMessageCommandReturnsNotFoundWhenSessionMissing()
    {
        var result = await Sender.Send(new CreateMyChatMessageCommand
        {
            ChatSessionId = Guid.NewGuid(),
            Message = "Hello"
        }, CancellationToken.None);

        result.IsNotFound.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }
}
