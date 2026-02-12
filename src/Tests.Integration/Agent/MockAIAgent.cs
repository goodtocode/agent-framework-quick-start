using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Tests.Integration.Agent;

public class MockAIAgent : AIAgent
{
    public override string? Name { get; }
    public override string? Description { get; }

    public override ValueTask<AgentSession> GetNewSessionAsync(CancellationToken cancellationToken = default)
        => new ValueTask<AgentSession>(new MockAgentSession("mock-session"));

    public override ValueTask<AgentSession> DeserializeSessionAsync(JsonElement serializedSession, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
        => new ValueTask<AgentSession>(new MockAgentSession("mock-deserialized-session"));

    protected override Task<AgentResponse> RunCoreAsync(IEnumerable<ChatMessage> messages, AgentSession? session = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
        => Task.FromResult<AgentResponse>(new MockAgentResponse("mock-response"));

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> messages, AgentSession? session = null, AgentRunOptions? options = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return new MockAgentResponseUpdate("mock-streaming-response");
        await Task.CompletedTask;
    }
}