using Goodtocode.AgentFramework.Core.Application.Playbooks;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks;

/// <summary>
/// Deterministic Record stage for the Document Review example playbook: shapes the finding into a
/// stored status/summary pair with no external persistence side effect.
/// </summary>
[PlaybookTool("document-review.deterministic.record")]
public sealed class DocumentReviewRecordTool : IRecordStepTool
{
    public string ToolName => "document-review.deterministic.record";

    public Task<ReviewRecord> RecordAsync(ReviewFinding finding, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(finding);

        var status = finding.Approved ? "Approved" : "Rejected";
        return Task.FromResult(new ReviewRecord(status, finding.Summary));
    }
}
