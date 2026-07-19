using Microsoft.Agents.AI;

namespace Goodtocode.AgentFramework.Tests.Integration.Mocks;

public class MockAgentSession : AgentSession
{
    public string SessionId { get; }

    public MockAgentSession(string sessionId = "mock-session") : base()
    {
        SessionId = sessionId;
        StateBag.SetValue("SessionId", sessionId);
    }
}
