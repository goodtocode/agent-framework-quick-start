using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;

/// <summary>
/// Defines the routing instructions for one registered agent tool.
/// </summary>
public sealed class AgentToolInstructionEntry
{
    /// <summary>The registered tool name.</summary>
    [Required]
    public string ToolName { get; set; } = string.Empty;

    /// <summary>The tool-specific instruction text.</summary>
    [Required]
    public string Instructions { get; set; } = string.Empty;

    /// <summary>The order in which this entry is appended to the agent instruction string.</summary>
    public int Order { get; set; }
}

/// <summary>
/// Defines reloadable global and per-tool instructions used by the chat agent.
/// </summary>
public sealed class AgentToolInstructionsOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "AgentToolInstructions";

    /// <summary>Instructions that apply to every tool invocation.</summary>
    public string GlobalPreamble { get; set; } = string.Empty;

    /// <summary>Instructions for individual registered tools.</summary>
    public List<AgentToolInstructionEntry> Tools { get; set; } = [];
}