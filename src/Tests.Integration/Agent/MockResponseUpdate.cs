using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Tests.Integration.Agent;

public class MockAgentResponseUpdate : AgentResponseUpdate
{
    public MockAgentResponseUpdate(string text)
        : base(ChatRole.Assistant, text)
    {
    }

    public MockAgentResponseUpdate(ChatRole? role, string text)
        : base(role, text)
    {
    }

    public MockAgentResponseUpdate(ChatRole? role, IList<AIContent>? contents)
        : base(role, contents)
    {
    }

    public MockAgentResponseUpdate()
        : base(ChatRole.Assistant, "mock-streaming-response")
    {
    }
}