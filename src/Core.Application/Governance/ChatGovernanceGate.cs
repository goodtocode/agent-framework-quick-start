using Goodtocode.Agent.Governance.Application;
using Goodtocode.Agent.Governance.Domain;

namespace Goodtocode.AgentFramework.Core.Application.Governance;

/// <summary>
/// Builds and enforces the governance envelope for one chat inference operation.
/// </summary>
public sealed class ChatGovernanceGate
{
    private readonly GovernanceEnforcer _enforcer = new(
        new EvaluationGovernancePromptComposer());

    /// <summary>
    /// Enforces governance and returns the system instruction for the chat inference.
    /// </summary>
    public GovernedEvaluationResult Enforce(
        IUserContext userContext,
        Guid chatSessionId,
        string prompt)
    {
        ArgumentNullException.ThrowIfNull(userContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        var correlationId = Guid.NewGuid();
        var request = new GovernanceEvaluationRequest
        {
            Governance = new EvaluationGovernanceRecord
            {
                PolicyProfileVersion = "v1",
                Observability = new ObservabilityRecord
                {
                    TraceId = correlationId.ToString("N"),
                    CorrelationId = correlationId,
                    EvidenceRefs = [GovernanceReference.Parse($"evidence://chat/{chatSessionId:N}")]
                },
                Repeatability = new RepeatabilityRecord
                {
                    ModelRef = "model://microsoft-agent-framework/chat-agent",
                    ModelVersion = typeof(ChatGovernanceGate).Assembly.GetName().Version?.ToString() ?? "unknown",
                    DeterministicReplaySupported = false,
                    Seed = null
                },
                Auditability = new AuditabilityRecord
                {
                    OwnerId = userContext.OwnerId,
                    TenantId = userContext.TenantId,
                    PrincipalDisplay = userContext.Email,
                    ToolRefs =
                    [
                        GovernanceReference.Parse("tool://agent-framework/chat-sessions"),
                        GovernanceReference.Parse("tool://agent-framework/actors"),
                        GovernanceReference.Parse("tool://agent-framework/chat-messages"),
                        GovernanceReference.Parse("tool://agent-framework/web-search")
                    ]
                },
                Defensibility = new DefensibilityRecord
                {
                    PoliciesApplied = [GovernanceReference.Parse("policy://goodtocode-agent-governance/v1")],
                    JustificationRefs = [GovernanceReference.Parse("justification://chat/user-request")],
                    ReasoningSummary = "Respond using applicable tools only when needed and preserve the user and tenant scope of every tool request.",
                    ConfidenceScore = 1
                }
            },
            ExistingSystemInstruction = "You are a helpful assistant operating in a governed chat application.",
            RepeatabilityPromptContent = prompt,
            RepeatabilityInputs = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["chatSessionId"] = chatSessionId,
                ["ownerId"] = userContext.OwnerId,
                ["tenantId"] = userContext.TenantId,
                ["prompt"] = prompt
            }
        };

        try
        {
            return _enforcer.Enforce(request);
        }
        catch (GovernanceValidationException exception)
        {
            throw new CustomValidationException(
                [.. exception.Issues.Select(issue => new ValidationFailure(issue.Field, issue.Message))]);
        }
    }
}