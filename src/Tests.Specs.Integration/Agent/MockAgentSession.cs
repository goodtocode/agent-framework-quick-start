using Microsoft.Agents.AI;
using System.Text.Json;

namespace Goodtocode.AgentFramework.Specs.Integration.Agent;

public class MockAgentSession : AgentSession
{
    public string SessionId { get; }

    public MockAgentSession(string sessionId = "mock-session")
    {
        SessionId = sessionId;
    }

    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
        => base.Serialize(jsonSerializerOptions);
}