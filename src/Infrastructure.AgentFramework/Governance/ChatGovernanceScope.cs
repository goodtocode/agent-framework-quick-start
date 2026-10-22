using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Playbook.Prompting.Governance;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Governance;

/// <summary>
/// Ambient governed scope read by the singleton governed chat client. The chat router opens it once per
/// turn with the tenant and the session Instruction, so the governed client applies the Instruction and the
/// per-message Preamble at the agent runtime boundary.
/// </summary>
public sealed class ChatGovernanceScope : IGovernedIntentScope
{
    private readonly AsyncLocal<State?> _current = new();

    /// <inheritdoc />
    public Guid TenantId => _current.Value?.TenantId ?? Guid.Empty;

    /// <inheritdoc />
    public string? Jurisdiction => _current.Value?.Jurisdiction;

    /// <inheritdoc />
    public AgentInstruction? Instruction => _current.Value?.Instruction;

    /// <summary>
    /// Opens a governed scope that lasts until the returned handle is disposed.
    /// </summary>
    public IDisposable Begin(Guid tenantId, AgentInstruction instruction, string? jurisdiction = null)
    {
        ArgumentNullException.ThrowIfNull(instruction);

        var previous = _current.Value;
        _current.Value = new State(tenantId, jurisdiction, instruction);
        return new Handle(this, previous);
    }

    private sealed record State(Guid TenantId, string? Jurisdiction, AgentInstruction Instruction);

    private sealed class Handle(ChatGovernanceScope owner, State? previous) : IDisposable
    {
        public void Dispose() => owner._current.Value = previous;
    }
}
