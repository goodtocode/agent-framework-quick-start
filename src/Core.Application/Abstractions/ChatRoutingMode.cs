namespace Goodtocode.AgentFramework.Core.Application.Abstractions;

/// <summary>
/// Selects how a chat reply is resolved.
/// </summary>
public enum ChatRoutingMode
{
    /// <summary>Uses deterministic routing, forced-tool inference, then an open agent turn.</summary>
    Routed = 0,

    /// <summary>Bypasses deterministic and forced-tool routing for diagnostics or explicit direct-agent requests.</summary>
    Direct = 1
}