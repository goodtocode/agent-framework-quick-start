using Goodtocode.AgentFramework.Core.Application.Playbooks;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks;

/// <summary>
/// Deterministic Collect stage for the Document Review example playbook: forwards the document
/// unchanged with no external source lookup.
/// </summary>
[PlaybookTool("document-review.deterministic.collect")]
public sealed class DocumentReviewCollectTool : ICollectStepTool
{
    public string ToolName => "document-review.deterministic.collect";

    public Task<ReviewEvidence> ExecuteAsync(ReviewRequest input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        return Task.FromResult(new ReviewEvidence(input.Document, Sources: []));
    }
}
