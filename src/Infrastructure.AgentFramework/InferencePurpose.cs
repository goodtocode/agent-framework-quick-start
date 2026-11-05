namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework;

/// <summary>Identifies the deployment contract required by an inference consumer.</summary>
public enum InferencePurpose
{
    /// <summary>Text-only conversation without application functions.</summary>
    Chat,
    /// <summary>Interactive application function selection and execution.</summary>
    Tool,
    /// <summary>Reasoning-intensive playbook inference without chat administration functions.</summary>
    ToolReasoning
}
