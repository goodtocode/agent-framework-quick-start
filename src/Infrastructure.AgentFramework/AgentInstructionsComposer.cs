using System.Text;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;
using Microsoft.Extensions.Options;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework;

/// <summary>
/// Composes MAF's single instruction string from reloadable global and per-tool configuration.
/// </summary>
public interface IAgentInstructionsComposer
{
    /// <summary>Builds the current agent instruction string.</summary>
    string Compose();
}

/// <summary>
/// Reads the current options snapshot whenever agent instructions are composed.
/// </summary>
public sealed class AgentInstructionsComposer(IOptionsMonitor<AgentToolInstructionsOptions> optionsMonitor) : IAgentInstructionsComposer
{
    private readonly IOptionsMonitor<AgentToolInstructionsOptions> _optionsMonitor = optionsMonitor;

    /// <inheritdoc />
    public string Compose()
    {
        var options = _optionsMonitor.CurrentValue;
        var instructions = new StringBuilder();

        instructions.AppendLine(ToolRoutingInstructions.AntiAnnouncementGuidance.Trim());
        instructions.AppendLine();

        if (!string.IsNullOrWhiteSpace(options.GlobalPreamble))
        {
            instructions.AppendLine(options.GlobalPreamble.Trim());
            instructions.AppendLine();
        }

        foreach (var tool in options.Tools.OrderBy(tool => tool.Order))
        {
            if (!string.IsNullOrWhiteSpace(tool.Instructions))
            {
                instructions.AppendLine(tool.Instructions.Trim());
                instructions.AppendLine();
            }
        }

        return instructions.ToString().TrimEnd();
    }
}