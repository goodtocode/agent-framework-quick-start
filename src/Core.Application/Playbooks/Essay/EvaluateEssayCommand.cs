using Goodtocode.AgentFramework.Core.Application.Chats;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Essay;

/// <summary>
/// Framework-agnostic abstraction over running the Essay Rubric Evaluation example playbook.
/// Implemented in <c>Infrastructure.AgentFramework</c> using the same shared MAF 3-node
/// Collect-&gt;Evaluate-&gt;Record graph as the Taxonomy playbook, proving that only the Evaluate
/// tool (deterministic vs. agentic) changes between the two, not the graph shape. The
/// Collect-stage input is the plain essay text string, matching the string-shaped Collect input
/// convention shared by every example playbook.
/// </summary>
public interface IEssayEvaluationRunner
{
    Task<PlaybookExecutionResult<EssayEvidence, EssayRubricFinding, EssayScorecardMaterialization>> EvaluateAsync(
        string essayText,
        CancellationToken cancellationToken,
        PlaybookReplayContext<EssayEvidence, EssayRubricFinding>? replayContext = null);
}

/// <summary>
/// Chat-integration adapter for the Essay playbook: resolves the essay text from an existing chat
/// message (reusing <c>GetMyChatMessageQuery</c> rather than introducing a bespoke essay schema)
/// before invoking the playbook with that plain text. This resolution step lives outside the
/// playbook's own CER boundary - the playbook itself never depends on chat storage.
/// </summary>
public sealed class EvaluateEssayCommand : IRequest<PlaybookExecutionResult<EssayEvidence, EssayRubricFinding, EssayScorecardMaterialization>>
{
    public required Guid ChatMessageId { get; init; }
}

public sealed class EvaluateEssayCommandHandler(ISender sender, IEssayEvaluationRunner runner)
    : IRequestHandler<EvaluateEssayCommand, PlaybookExecutionResult<EssayEvidence, EssayRubricFinding, EssayScorecardMaterialization>>
{
    private readonly ISender _sender = sender;
    private readonly IEssayEvaluationRunner _runner = runner;

    public async Task<PlaybookExecutionResult<EssayEvidence, EssayRubricFinding, EssayScorecardMaterialization>> Handle(
        EvaluateEssayCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var message = await _sender.Send(new GetMyChatMessageQuery { Id = request.ChatMessageId }, cancellationToken);
        if (message is null)
        {
            throw new CustomNotFoundException($"No chat message was found with id '{request.ChatMessageId}' to use as an essay submission.");
        }

        return await _runner.EvaluateAsync(message.Content, cancellationToken);
    }
}
