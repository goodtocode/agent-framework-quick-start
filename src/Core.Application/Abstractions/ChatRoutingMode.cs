namespace Goodtocode.AgentStart.Core.Application.Abstractions;

/// <summary>
/// Selects how a chat reply is resolved.
/// </summary>
public enum ChatRoutingMode
{
    /// <summary>Uses deterministic routing, then purpose-specific function inference without open-agent retries.</summary>
    Routed = 0,

    /// <summary>Uses text-only inference without classification or application functions.</summary>
    Direct = 1
}