# Feature: Playbook — Essay Rubric Evaluation

## Overview
A hybrid example Playbook: Collect and Record stages are deterministic, while the Evaluate stage
is agentic — the model scores an essay against four weighted rubric criteria, and the overall
score is computed deterministically from those model-reported per-criterion scores. This is the
**recommended real-world pattern**: deterministic evidence retrieval and deterministic
materialization surrounding a single, tightly-scoped reasoning stage. It also demonstrates how an
existing capability (chat message storage) can feed a Playbook's plain-`string` Collect input
without introducing any Playbook-specific schema for that integration.

## Business Problem
Most real agentic capabilities are not "fully deterministic" or "fully agentic" — they need a model
for the one step that genuinely requires judgment (here, qualitative essay scoring) while keeping
everything else fast, cheap, and replayable. Developers need a worked example of this hybrid shape,
including how to resolve the model's input from existing application data (a chat message) without
coupling the Playbook itself to chat storage.

## Business Value
- Demonstrates the recommended production pattern: isolate non-determinism to exactly the stage
  that needs it.
- Shows rubric-weighted scoring with per-criterion model output reconciled against a versioned,
  shared `PlaybookKnowledge` rubric, then deterministically combined into one overall score and
  letter grade — keeping the arithmetic itself fully replayable even though the inputs to it are
  not.
- Demonstrates the correct boundary for integrating existing application data (chat messages) with
  a Playbook: resolution happens in the command handler, never inside the Playbook's Collect-stage
  tool, keeping the Playbook itself reusable outside the chat subsystem.

## User Story
As a developer evaluating this quick-start template, I want a working example of a hybrid
deterministic/agentic Playbook that scores an essay against a weighted rubric, so that I understand
how to combine model judgment with deterministic aggregation and how to wire an existing data
source (chat messages) into a Playbook's plain-string Collect input.

## Related Ontology Concepts
- **Chat Message** (`docs/product/sprint-0/ontology.md`) — the optional real-world source of this
  Playbook's Collect-stage input, resolved via `GetMyChatMessageQuery` outside the Playbook
  boundary.
- **Agent** — an `AIAgent` instance backs the Evaluate stage tool only.
- **Governed Inference** — every stage, including the one agentic stage, produces a governance
  record.
- See `docs/governance/playbook-workflow-types.md` for the full three-workflow comparison and the
  shared "Unified Collect / Evaluate / Record Shape" convention this feature follows.

## User Flow
Two entry points exist:

1. **Direct runner call** — `IEssayEvaluationRunner.EvaluateAsync(string essayText, CancellationToken)`
   for callers who already have essay text in hand (no chat dependency).
2. **Chat-integration adapter** — `EvaluateEssayCommand { ChatMessageId }` sent via `ISender`.
   `EvaluateEssayCommandHandler` resolves the chat message through `GetMyChatMessageQuery`, extracts
   its `Content`, throws `CustomNotFoundException` if the message does not exist, and only then
   calls the runner with the plain essay-text string. The Playbook itself never depends on chat
   storage.

Neither entry point is wired to a chat tool or UI page today; both are proven through integration
tests. See [API Changes](#api-changes) for what adding an endpoint would require.

## Acceptance Criteria
- [x] Collect stage is a deterministic pass-through: it wraps the input `string` and a UTC
      timestamp into `EssayEvidence`, with no LLM involvement.
- [x] Evaluate stage scores the essay against exactly the four criteria defined by the active
      rubric, rejecting model responses that omit or add criteria.
- [x] The overall score is a deterministic rubric-weighted sum of the model's per-criterion scores,
      never itself produced by the model.
- [x] Record stage deterministically bands the overall score into a letter grade (A–F) with no LLM
      involvement.
- [x] `EvaluateEssayCommandHandler` resolves essay text from an existing chat message without
      introducing any essay-specific chat schema, and throws a not-found error for a missing
      message.
- [x] Every stage execution produces a valid `EvaluationGovernanceRecord`, with
      `Repeatability.DeterministicReplaySupported == true` for Collect and Record and `== false`
      for Evaluate.
- [x] The Collect-stage input is a plain `string` — the same shape convention used by the SQL
      Statistics and Taxonomy Playbooks.

## Functional Requirements
- Collect-stage input is the essay text as a plain `string` (no bespoke request wrapper type at the
  Playbook boundary).
- The Evaluate-stage prompt is assembled from `PlaybookKnowledge.Instruction` plus a criteria list
  rendered dynamically from `EvaluationRubric.Criteria` — never hardcoded in the prompt string.
- The model must return exactly one score per criterion (strict JSON); a mismatched count or a
  missing criterion throws `InvalidOperationException` rather than silently defaulting.
- The overall score is computed as `Σ(criterion.Score × criterion.Weight)` entirely in application
  code, never requested from or trusted from the model.
- Letter-grade banding (`>= 0.9` → A, `>= 0.8` → B, `>= 0.7` → C, `>= 0.6` → D, else F) is a pure
  deterministic function of the overall score.

## Domain Changes
None. This Playbook does not introduce new `Core.Domain` entities; its optional persistence reuses
the shared `PlaybookMaterializationEntity` (see [Data Requirements](#data-requirements)). The
chat-integration path reads existing `ChatMessageEntity`/`ChatSessionEntity` data but does not
modify it.

## Application Changes
All types live in `Core.Application/Playbooks/Essay/`:

- `EssayEvidence(string EssayText, DateTimeOffset CollectedUtc)` — Collect-stage output.
- `EssayKnowledgeHolder` — wraps a `Goodtocode.Agents.Playbook.Execution.PlaybookKnowledge` built
  from a single 0–1 `ContinuousEvaluationScale` applied to four weighted `EvaluationCriterion`s
  (Thesis clarity 0.25, Evidence and support 0.35, Organization 0.25, Grammar and mechanics 0.15).
  This is the same `PlaybookKnowledge`/`EvaluationRubric` type every example Playbook uses to carry
  its criteria — see `docs/governance/playbook-workflow-types.md`.
- `EssayCriterionScore(string Criterion, double Score, string Reason)` and
  `EssayRubricFinding(string EssayText, IReadOnlyList<EssayCriterionScore> CriterionScores, double OverallScore, string RubricVersion)`
  — Evaluate-stage output.
- `EssayScorecardMaterialization(double OverallScore, string Grade, IReadOnlyList<EssayCriterionScore> Breakdown, string Summary)`
  — Record-stage output; implements `IPlaybookMaterializationSummary`.
- `IEssayEvaluationRunner` (in `Infrastructure.AgentFramework`, since it depends on
  `Microsoft.Agents.AI.Workflows`) — `EvaluateAsync(string essayText, CancellationToken) -> Task<PlaybookExecutionResult<...>>`.
- `EvaluateEssayCommand { ChatMessageId }` / `EvaluateEssayCommandHandler(ISender, IEssayEvaluationRunner)`
  — the chat-integration adapter described in [User Flow](#user-flow).

## Agent and Tool Contract
Only the Evaluate stage uses an `AIAgent`. The three stage tools, registered in
`Infrastructure.AgentFramework/Playbooks/Essay/` and discovered by `[PlaybookTool("...")]`
attribute via `AddEssayPlaybookTools()`:

| Stage | Tool name | Implementation |
|-------|-----------|-----------------|
| Collect | `essay.deterministic.collect` | `EssayCollectTool` — pure pass-through wrapping input text + timestamp into `EssayEvidence`. No dependencies. |
| Evaluate | `essay.agentic.evaluate` | `EssayEvaluateAgentTool` — prompts the model to score each rubric criterion as strict JSON, validates completeness against `EssayKnowledgeHolder.Knowledge.Rubric.Criteria`, computes the weighted overall score. |
| Record | `essay.deterministic.record` | `EssayRecordTool` — bands the overall score into a letter grade and shapes the plain-text summary. No LLM involvement. |

This mirrors the exact same MAF three-node graph shape as the
[Taxonomy](./feature-playbook-taxonomy.md) Playbook; only the Evaluate tool's implementation
(agentic vs. deterministic) differs, proving the graph never needs to change shape when a single
stage's determinism changes.

## Prompt, Model, and Memory
- The Evaluate-stage prompt is assembled from the rubric's instruction text, its criteria list
  (`CriterionId: Description` per line), and the full essay text, instructing strict JSON scoring
  output (`{"scores": [{"criterion": ..., "score": ..., "reason": ...}, ...]}`).
- No cross-invocation memory; each Evaluate call is a single, self-contained model turn scoped to
  one essay's full text.
- Governed system instruction enforcement happens at the runtime boundary per
  `docs/governance/ai-policy.md`; this feature does not bypass or duplicate that mechanism.

## Observability and Audit
Every stage execution is recorded by `EssayGovernanceActivityRecorder` (a
`PlaybookGovernanceActivityRecorder<EssayEvidence, EssayRubricFinding, EssayScorecardMaterialization>`),
producing a full `EvaluationGovernanceRecord` per stage, per the four governance pillars in
`docs/governance/ai-policy.md`. Because the Collect and Record tool names match the
`*.deterministic.*` convention and the Evaluate tool name matches `*.agentic.*`,
`Repeatability.DeterministicReplaySupported` is `true`, `false`, `true` for Collect, Evaluate,
Record respectively.

## Runtime States
- **Successful**: all three stages complete; result carries evidence, finding, and materialization
  (overall score, letter grade, per-criterion breakdown, and summary).
- **Failed**:
  - `EvaluateEssayCommandHandler` throws `CustomNotFoundException` when `ChatMessageId` does not
    resolve to an existing message.
  - `EssayEvaluateAgentTool` throws `InvalidOperationException` when the model's response scores
    the wrong number of criteria or omits a required criterion.
- Not applicable: streaming, pending/loading, or unauthorized states — this Playbook has no chat/UI
  surface today (see [User Flow](#user-flow)).

## Evaluation and Quality Criteria
Rubric *completeness* is enforced structurally (wrong criterion count/missing criterion throws).
Per-criterion score *quality* (e.g., whether a 0.9 thesis-clarity score is actually justified) is not
independently evaluated — this is a noted opportunity for an evaluator/replay baseline, not a
current governance gap (every score is still captured, attributed, and auditable).

## UI Changes
None. Not yet wired to a chat surface or page.

## API Changes
None today. To expose this Playbook over HTTP, add a minimal endpoint or controller in
`Presentation.Api` that sends `EvaluateEssayCommand` through `ISender` (for the chat-message path)
or resolves `IEssayEvaluationRunner` directly (for a raw-text path), following the same
authorization/tenant-scoping conventions as other endpoints in that project.

## Data Requirements
No dedicated schema beyond what already exists for chat messages. The Record-stage materialization
can optionally be persisted through the shared `SavePlaybookMaterializationCommand`
(`Core.Application/Playbooks/Persistence`) into the single `PlaybookMaterializations` table — no
bespoke table or migration is required for this Playbook specifically.

## Security Requirements
`EvaluateEssayCommand` is a `UserScopedRequest`-style flow through `GetMyChatMessageQuery`, so it
inherits the existing `My`-request owner/tenant scoping — a user cannot resolve another user's chat
message through this command. Essay text submitted directly (bypassing chat) should be treated per
`docs/governance/ai-policy.md` sensitive-data guidance by any adopter wiring a public-facing
endpoint.

## Testing Requirements
- Integration: `Tests.Integration/Playbooks/EssayPlaybookTests.cs` and
  `EssayPlaybookStepDefinitions.cs` (`EssayPlaybook.feature`) cover:
  - Tool discovery by attribute for all three stages.
  - End-to-end scoring via the direct runner path with queued model responses, asserting the
    computed overall score (`0.9*0.25 + 0.8*0.35 + 0.85*0.25 + 1.0*0.15 = 0.8675`), letter grade
    `"B"`, and exactly 3 governance records with the expected per-stage replay flags
    (`true, false, true`).
  - `EvaluateEssayCommand` resolving essay text from a seeded chat message end-to-end through
    `ISender` before scoring.

## Risks
- The four rubric criteria and their weights are illustrative defaults; template adopters must
  replace `EssayKnowledgeHolder.V1` with their own versioned rubric before relying on this in
  production.
- No retry/repair loop exists for malformed or incomplete model scoring JSON today; it surfaces as
  an immediate failure.

## Out Of Scope
- Exposing this Playbook via a chat tool, REST endpoint, or UI page.
- Persisting results by default (persistence is opt-in via `SavePlaybookMaterializationCommand`).
- Automatic retry or self-correction of malformed/incomplete model responses.
- Independent quality evaluation of per-criterion score justifications.

## Definition Of Done
- [x] Implementation complete (`Core.Application` + `Infrastructure.AgentFramework`).
- [x] Tests added/updated and passing (`Tests.Integration`, 136/136 solution-wide).
- [x] Build passes with 0 warnings/errors.
- [x] Documentation updated (this file, `docs/governance/playbook-workflow-types.md`, `README.md`).
- [x] Acceptance criteria satisfied.
