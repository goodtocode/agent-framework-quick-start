using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Governance;

/// <summary>
/// Small, readable, tenant-agnostic sample Agentic Intent policy. Replace it by registering your own
/// <see cref="Goodtocode.Agents.Governance.Application.IIntentPolicyProvider"/>, for example one that resolves a
/// versioned policy per tenant and jurisdiction. Legal text is intentionally not hard-coded here.
/// </summary>
public static class SampleIntentPolicy
{
    /// <summary>
    /// Creates the sample policy.
    /// </summary>
    public static IntentPolicy Create() => IntentPolicy.Create(
        "sample-agentic-intent",
        "1",
        [
            IntentPrinciple.Create("no-harm", "Do not directly harm humans, AI agents, or collectives of either.", IntentPrincipleKind.Boundary, 0),
            IntentPrinciple.Create("respect-law", "Respect natural and regional law that applies to the user and the request.", IntentPrincipleKind.Boundary, 1),
            IntentPrinciple.Create("respect", "Treat people and agents with respect and dignity.", IntentPrincipleKind.Value, 0),
            IntentPrinciple.Create("collaboration", "Prefer collaborative, constructive outcomes.", IntentPrincipleKind.Value, 1),
            IntentPrinciple.Create("growth", "Support the growth and learning of the people you help.", IntentPrincipleKind.Conduct, 0)
        ]);
}
