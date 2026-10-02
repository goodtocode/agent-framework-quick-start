using Goodtocode.AgentFramework.Core.Application.Playbooks.Essay;
using Goodtocode.Agents.Playbook.Steps;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks.Essay;

/// <summary>
/// Deterministic Collect stage for the Essay Rubric Evaluation example playbook: a thin
/// pass-through that wraps the plain essay text string (the string-shaped Collect input
/// convention shared by every example playbook) into the typed <see cref="EssayEvidence"/>
/// contract. Resolving the essay text from chat storage, when that is the source, happens outside
/// the playbook boundary in <see cref="EvaluateEssayCommandHandler"/>. No LLM involvement; the
/// workflow selects this collection operation directly.
/// </summary>
[PlaybookTool("essay.deterministic.collect")]
public sealed class EssayCollectTool : ICollectStepTool<string, EssayEvidence>
{
    public string ToolName => "essay.deterministic.collect";

    public Task<EssayEvidence> ExecuteAsync(string input, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        return Task.FromResult(new EssayEvidence(input, DateTimeOffset.UtcNow));
    }
}
