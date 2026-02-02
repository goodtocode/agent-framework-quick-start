using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Specs.Integration.Agent;

public class MockAgentResponse : AgentResponse
{
    public MockAgentResponse(string text)
    {
        // AgentResponse.Text is derived from Messages, so we create a message with the desired text.
        this.Messages = new List<ChatMessage> { new ChatMessage(ChatRole.Assistant, text) };
    }

    public MockAgentResponse(IEnumerable<ChatMessage> messages)
    {
        this.Messages = new List<ChatMessage>(messages);
    }

    public MockAgentResponse()
    {
        // Default: empty response
    }
}