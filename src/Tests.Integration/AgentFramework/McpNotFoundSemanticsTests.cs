using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

namespace Goodtocode.AgentFramework.Tests.Integration.AgentFramework;

[TestClass]
public class McpNotFoundSemanticsTests : TestBase
{
    [TestMethod]
    public async Task ActorsToolGetActorByIdReturnsNullWhenMissing()
    {
        var sut = new ActorsTool(ServiceProvider);

        var result = await sut.GetActorByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeNull();
    }

    [TestMethod]
    public async Task ChatSessionsToolUpdateTitleReturnsNullWhenSessionMissing()
    {
        var sut = new MyChatSessionsTool(ServiceProvider);

        var result = await sut.UpdateChatSessionTitleAsync(Guid.NewGuid(), "Updated Title", CancellationToken.None);

        result.ShouldBeNull();
    }
}
