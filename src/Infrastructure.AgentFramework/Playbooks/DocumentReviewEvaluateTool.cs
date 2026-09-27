using Goodtocode.AgentFramework.Core.Application.Playbooks;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks;

/// <summary>
/// Deterministic Evaluate stage for the Document Review example playbook: a fixed, rule-based
/// stand-in for an agentic reviewer. A follow-up workflow can register an agentic tool under a
/// different key for the same Evaluate stage without changing Collect or Record.
/// </summary>
[PlaybookTool("document-review.deterministic.evaluate")]
public sealed class DocumentReviewEvaluateTool : IEvaluateStepTool
{
    public string ToolName => "document-review.deterministic.evaluate";

    public Task<ReviewFinding> EvaluateAsync(ReviewEvidence evidence, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        var approved = !string.IsNullOrWhiteSpace(evidence.Document);
        var summary = approved
            ? "Document contains reviewable content."
            : "Document is empty and cannot be approved.";

        return Task.FromResult(new ReviewFinding(approved, summary));
    }
}
