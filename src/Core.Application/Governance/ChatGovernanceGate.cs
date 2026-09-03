using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Domain.Governance;
using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.AgentFramework.Core.Application.Governance;

public sealed class ChatGovernanceGate
{
    private readonly GovernanceEnforcer enforcer = new(new EvaluationGovernancePromptComposer());

    public Goodtocode.Agents.Governance.Application.GovernedEvaluationResult Enforce(
        IRlsContext userContext,
        string prompt)
    {
        ArgumentNullException.ThrowIfNull(userContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        var correlationId = Guid.NewGuid();
        return enforcer.Enforce(new GovernanceEvaluationRequest
        {
            Governance = new EvaluationGovernanceRecord
            {
                PolicyProfileVersion = "chat-v1",
                Observability = new ObservabilityRecord
                {
                    TraceId = correlationId.ToString("N"),
                    CorrelationId = correlationId,
                    EvidenceRefs =
                    [
                        GovernanceReference.Parse($"evidence://chat/{correlationId:N}")
                    ]
                },
                Repeatability = new RepeatabilityRecord
                {
                    ModelRef = "agent://configured-chat-agent",
                    ModelVersion = "configured",
                    DeterministicReplaySupported = false
                },
                Auditability = new AuditabilityRecord
                {
                    OwnerId = userContext.OwnerId,
                    TenantId = userContext.TenantId,
                    PrincipalDisplay = $"owner:{userContext.OwnerId:N}",
                    ToolRefs = []
                },
                Defensibility = new DefensibilityRecord
                {
                    PoliciesApplied = [GovernanceReference.Parse("policy://chat/governance-v1")],
                    JustificationRefs =
                    [
                        GovernanceReference.Parse("justification://chat/governance-v1")
                    ],
                    ReasoningSummary = "Chat responses require an attributable, traceable, and repeatable governance envelope.",
                    ConfidenceScore = null
                }
            },
            ExistingSystemInstruction = "You are a helpful AI assistant.",
            RepeatabilityPromptContent = prompt,
            RepeatabilityInputs = new Dictionary<string, object?>
            {
                ["chatPrompt"] = prompt
            }
        });
    }

    public ChatGovernanceEntity CreatePersistenceRecord(
        Guid ownerId,
        Guid tenantId,
        Guid chatSessionId,
        Goodtocode.Agents.Governance.Application.GovernedEvaluationResult governed)
    {
        ArgumentNullException.ThrowIfNull(governed);

        var governance = governed.Governance;
        var repeatability = governance.Repeatability;
        var observability = governance.Observability;
        var auditability = governance.Auditability;
        var defensibility = governance.Defensibility;
        static string Serialize(object? value) => System.Text.Json.JsonSerializer.Serialize(value);

        return ChatGovernanceEntity.Create(
            ownerId,
            tenantId,
            chatSessionId,
            governance.PolicyProfileVersion,
            observability.TraceId,
            observability.CorrelationId,
            auditability.PrincipalDisplay,
            repeatability.ModelRef,
            repeatability.ModelVersion,
            governed.PromptHash,
            governed.InputHash,
            repeatability.DeterministicReplaySupported,
            governed.PromptContext.SystemInstruction,
            Serialize(governed.PromptContext.Metadata),
            Serialize(observability.EvidenceRefs),
            Serialize(auditability.ToolRefs),
            Serialize(defensibility.PoliciesApplied),
            Serialize(defensibility.JustificationRefs),
            defensibility.ReasoningSummary,
            defensibility.ConfidenceScore is null ? null : (decimal)defensibility.ConfidenceScore.Value);
    }
}
