using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Tests.Integration.Mocks;

public class MockAIAgent : AIAgent
{
    public IReadOnlyList<ChatMessage> LastMessages { get; private set; } = [];
    public IReadOnlyList<AgentRunOptions?> RunOptions { get; private set; } = [];
    public int RunCount { get; private set; }
    public bool ThrowOnForcedToolRun { get; set; }

    protected override ValueTask<AgentSession> CreateSessionCoreAsync(CancellationToken cancellationToken = default)
        => new(new MockAgentSession("mock-session"));

    protected override ValueTask<JsonElement> SerializeSessionCoreAsync(AgentSession session, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
    {
        using var doc = JsonDocument.Parse("{}");
        return new ValueTask<JsonElement>(doc.RootElement.Clone());
    }

    protected override ValueTask<AgentSession> DeserializeSessionCoreAsync(JsonElement serializedState, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
        => new(new MockAgentSession("mock-deserialized-session"));

    protected override Task<AgentResponse> RunCoreAsync(IEnumerable<ChatMessage> messages, AgentSession? session = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        LastMessages = [.. messages];
        RunOptions = [.. RunOptions, options];
        RunCount++;
        if (ThrowOnForcedToolRun && options is ChatClientAgentRunOptions)
        {
            throw new InvalidOperationException("Forced-tool inference failed.");
        }

        return Task.FromResult<AgentResponse>(new MockAgentResponse("mock-response"));
    }

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> messages, AgentSession? session = null, AgentRunOptions? options = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return new MockAgentResponseUpdate("mock-streaming-response");
        await Task.CompletedTask;
    }
}
