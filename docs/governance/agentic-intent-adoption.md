# Agentic Intent: Instruction and Preamble

Agentic Intent is the first governance **policy**, applied beside the four pillars (Observability, Repeatability, Auditability, Defensibility). Principles are `Boundary`, `Value` and `Conduct`, with precedence Boundary > Value > Conduct > persona > user request.

## How the template applies it

| Concept | Cadence | Where |
|---|---|---|
| Instruction | Once per chat session | `ChatMessageIntentRouter` opens `ChatGovernanceScope.Begin(tenantId, instruction)` with the persona plus governed directives |
| Preamble | Before every user message | `GovernedChatClient` (via `UseGovernedIntent`) in `ConfigureServices.cs`; never rendered as a chat bubble |
| Policy identity | Every governed inference | `ChatGovernanceGate.Enforce` adds `policy://agentic-intent/{id}/{version}/{hash}` to `Defensibility.PoliciesApplied`; playbook steps record the same reference |

A change to the policy text or version changes the hash and is reported as drift.

## Configuration

```json
"GovernedIntent": { "IntentRequired": true, "PreambleMode": "Compact" }
```

- `IntentRequired`: fail the call when no policy resolves.
- `PreambleMode`: `Compact` (principle ids) or `Full` (statements).

## Replacing the sample policy

`Infrastructure.AgentFramework/Governance/SampleIntentPolicy.cs` is a small, tenant-agnostic sample. Replace the `IIntentPolicyProvider` registration in `ConfigureServices.cs` with your own provider (for example one that resolves a versioned policy per tenant and jurisdiction from storage). Do not hard-code legal text in the template; supply it as versioned policy.

Prompting is guidance, not enforcement. Register the governed client as the default `IChatClient` so every call goes through it.
