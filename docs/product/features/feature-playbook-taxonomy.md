# Feature: Playbook — Taxonomy Extraction and Classification

## Overview
A fully agentic example Playbook: every Collect → Evaluate → Record (CER) stage is backed by a
model call. Given free-text, the model first extracts candidate subject-matter terms (Collect),
then classifies those terms into exactly one of a fixed set of taxonomy categories against a
versioned rubric (Evaluate), and finally summarizes the outcome (Record). It demonstrates the
opposite end of the CER spectrum from [SQL Statistics Classification](./feature-playbook-sql-statistics.md):
maximum model involvement, with governance and rubric-conformance validation still fully enforced
at every stage.

## Business Problem
Developers adopting this template need to see end-to-end agentic reasoning — not just a single
chat completion, but a multi-stage reasoning pipeline where each stage's model output is validated
against a typed contract and a versioned rubric before it is trusted. Without a worked example,
teams tend to either skip validation entirely (trusting raw model JSON) or over-engineer bespoke
validation per capability.

## Business Value
- Shows the full Microsoft Agent Framework (MAF) `WorkflowBuilder`/`InProcessExecution` graph
  pattern (`Collect -> Evaluate -> Record`, three nodes) reused verbatim from the shared graph
  builder, proving the graph shape itself never needs to change when stage implementations become
  agentic.
- Demonstrates strict output validation: the model's chosen category is rejected with a clear
  `InvalidOperationException` if it does not match a label in the active rubric's
  `DiscreteEvaluationScale`, preventing silent rubric drift.
- Reinforces governance expectations for non-deterministic stages: every stage here records
  `Repeatability.DeterministicReplaySupported == false`, showing adopters exactly how that signal
  differs from the deterministic SQL Statistics example.

## User Story
As a developer evaluating this quick-start template, I want a working example of a fully agentic,
rubric-governed classification Playbook, so that I understand how to validate model output against
a versioned rubric and wire a three-stage MAF workflow graph correctly.

## Related Ontology Concepts
- **Agent** (`docs/product/sprint-0/ontology.md`) — an `AIAgent` instance backs both the Collect
  and Evaluate stage tools here.
- **Governed Inference** — every stage, including the two agentic ones, produces a governance
  record.
- **Tool** — each CER stage is still a typed, scoped tool; "agentic" only changes what is *inside*
  the tool, never the stage contract shape.
- See `docs/governance/playbook-workflow-types.md` for the full three-workflow comparison and the
  shared "Unified Collect / Evaluate / Record Shape" convention this feature follows.

## User Flow
Not a chat-driven user flow today. Exercised programmatically through
`ITaxonomyClassificationRunner.ClassifyAsync(string text, CancellationToken)` and proven via
integration tests rather than an exposed API/UI surface. See [API Changes](#api-changes) for what
adding an endpoint would require.

## Acceptance Criteria
- [x] Collect stage extracts candidate terms from free text via the model, validated and mapped
      into `TaxonomyEvidence` before crossing the stage boundary.
- [x] Evaluate stage classifies the extracted terms into exactly one of five named categories
      (Finance, Healthcare, Technology, Legal, Other) defined by a versioned
      `DiscreteEvaluationScale`.
- [x] An unrecognized category returned by the model causes the Evaluate stage to throw rather than
      silently accept an invalid classification.
- [x] Record stage produces a materialization with a plain-text summary.
- [x] Every stage execution produces a valid `EvaluationGovernanceRecord` with
      `Repeatability.DeterministicReplaySupported == false` for all three stages.
- [x] The Collect-stage input is a plain `string` — the same shape convention used by the SQL
      Statistics and Essay Playbooks.

## Functional Requirements
- Collect-stage input is the source text as a plain `string` (no bespoke request wrapper type).
- The model is prompted to respond with strict JSON (`{"terms": [...]}`), parsed via
  `AgentStructuredOutputParser.ParseOrThrow<T>`, which throws a descriptive error on malformed
  responses rather than returning null/default silently.
- The categories available to the Evaluate-stage model prompt are generated dynamically from the
  active rubric's `DiscreteEvaluationScale.Levels` — never hardcoded in the prompt string.
- The model's chosen category is matched case-insensitively against `EvaluationScaleLevel.Label`;
  no match throws.

## Domain Changes
This Playbook's catalog definition is persisted as a `PlaybookEntity` (`Key: taxonomy`) with
three `PlaybookStepEntity` children (`Collect`/`Evaluate`/`Record`), each carrying the step's
`ActionFormat` and static `ActionDefinition` (the term-extraction prompt, the taxonomy-category
rubric, and the summary/record prompt, respectively). Every run is persisted as a
`PlaybookExecutionEntity` — see [Data Requirements](#data-requirements) and
`docs/governance/playbook-workflow-types.md` ("Persistence: Playbook Catalog vs. Playbook
Execution") for the full entity model shared by all three example Playbooks.

## Application Changes
All types live in `Core.Application/Playbooks/Taxonomy/`:

- `TaxonomyEvidence(string SourceText, IReadOnlyList<string> ExtractedTerms, DateTimeOffset CollectedUtc)`
  — Collect-stage output.
- `TaxonomyKnowledgeHolder` — wraps a `Goodtocode.Agents.Playbook.Execution.PlaybookKnowledge`
  built from a `DiscreteEvaluationScale` with five named `EvaluationScaleLevel`s (category name +
  description each). This is the same `PlaybookKnowledge`/`EvaluationRubric` type every example
  Playbook uses to carry its criteria — see `docs/governance/playbook-workflow-types.md`.
- `TaxonomyFinding(string SourceText, string Category, double Confidence, string Rationale, string TaxonomyVersion)`
  — Evaluate-stage output.
- `TaxonomyMaterialization(string Category, double Confidence, string Summary)` — Record-stage
  output; implements `IPlaybookMaterializationSummary`.
- `ITaxonomyClassificationRunner` / `TaxonomyClassificationRunner` (in
  `Infrastructure.AgentFramework`, since it depends on `Microsoft.Agents.AI.Workflows`) — the
  framework-facing entry point: `ClassifyAsync(string text, CancellationToken) -> Task<PlaybookExecutionResult<...>>`.
- `ClassifyTaxonomyCommand` / `ClassifyTaxonomyCommandHandler` — a typed mediator request wrapping
  the runner for callers that prefer the application request pipeline over direct runner
  injection.

## Agent and Tool Contract
Both Collect and Evaluate are backed by the same injected `AIAgent`. The three stage tools,
registered in `Infrastructure.AgentFramework/Playbooks/Taxonomy/` and discovered by
`[PlaybookTool("...")]` attribute via `AddTaxonomyPlaybookTools()`:

| Stage | Tool name | Implementation |
|-------|-----------|-----------------|
| Collect | `taxonomy.agentic.collect` | `TaxonomyCollectAgentTool` — prompts the model to extract terms as strict JSON, validates via `AgentStructuredOutputParser`. |
| Evaluate | `taxonomy.agentic.evaluate` | `TaxonomyEvaluateAgentTool` — builds the category list from `DiscreteEvaluationScale.Levels`, prompts the model to classify, validates the chosen category against the scale. |
| Record | `taxonomy.agentic.record` | Summarizes the classification outcome as plain text. |

No tool here is independently LLM-selectable mid-conversation; the workflow graph drives fixed
stage order (`Collect -> Evaluate -> Record`), matching the "agentic" orchestration row in the
workflow-type comparison table.

## Prompt, Model, and Memory
- Collect-stage prompt instructs strict single-purpose JSON extraction (`{"terms": [...]}`), no
  conversational framing.
- Evaluate-stage prompt is assembled from `PlaybookKnowledge.Instruction` plus the dynamically
  rendered category list plus the Collect-stage's extracted terms, instructing strict JSON
  classification output (`{"category": ..., "confidence": ..., "rationale": ...}`).
- No cross-invocation memory; each stage call is a single, self-contained model turn scoped to its
  own typed input.
- Governed system instruction enforcement happens at the runtime boundary per
  `docs/governance/ai-policy.md`; this feature does not bypass or duplicate that mechanism.

## Observability and Audit
Every stage execution is recorded by `TaxonomyGovernanceActivityRecorder` (a
`PlaybookGovernanceActivityRecorder<TaxonomyEvidence, TaxonomyFinding, TaxonomyMaterialization>`),
producing a full `EvaluationGovernanceRecord` per stage, per the four governance pillars in
`docs/governance/ai-policy.md`. Because every stage tool name matches the `*.agentic.*` naming
convention, `Repeatability.DeterministicReplaySupported` is `false` for all three stages.

## Runtime States
- **Successful**: all three stages complete; result carries evidence, finding, and materialization.
- **Failed**: the model returns malformed JSON (`AgentStructuredOutputParser.ParseOrThrow` throws),
  or returns a category not present in the active rubric (`TaxonomyEvaluateAgentTool` throws
  `InvalidOperationException`).
- Not applicable: streaming, pending/loading, or unauthorized states — this Playbook has no chat/UI
  surface today (see [User Flow](#user-flow)).

## Evaluation and Quality Criteria
Rubric conformance is enforced structurally (invalid categories throw), not scored. No evaluator or
replay baseline exists yet for the *quality* of model-extracted terms or classification confidence
— this is a noted opportunity, not a current gap in governance coverage (classification validity is
already enforced; term-extraction *quality* is not yet separately evaluated).

## UI Changes
The `TaxonomyPlaybookPage` (`Presentation.Web/Features/Playbooks`) runs this Playbook from a
free-text input, then renders the shared `PlaybookResultPanel`: "Rerun"/"Recall"/"Replay"
repeatability controls (`PlaybookReplayControls`, each with an inline explanation of what it
does), a Collect/Evaluate/Record toggle, a summary card, and a
step-detail panel. The step-detail panel fetches this Playbook's catalog entry
(`GetPlaybookByKeyAsync("taxonomy")`) to show each step's persisted `Description` and
`ActionDefinition` (the extraction/classification prompts) alongside that run's dynamic
input/output.

## Repeatability (Recall / Replay)
Because every stage of this Playbook is agentic, Recall and Replay matter most here: Recall lets
a user see the exact prior Finding without spending any tokens, and Replay proves the
Evaluate/Record prompts reproduce the same classification from the same (recalled) extracted
terms, without re-running the more token-heavy Collect stage. Unlike SQL Statistics, this
Playbook's `TaxonomyClassificationRunner` does not call the package's `PlaybookExecutor`
directly — it passes the `PlaybookReplayContext<TaxonomyEvidence, TaxonomyFinding>` through to the
repo-owned `PlaybookWorkflowGraphExecutor<...>`, which bypasses the MAF 3-node graph entirely for
Recall/Replay and instead calls only the remaining stage tool(s)
(`IRecordStepTool<,>`/`IEvaluateStepTool<,>`, resolved to `TaxonomyRecordAgentTool`/
`TaxonomyEvaluateAgentTool`) directly against the recalled Evidence/Finding, while still emitting
the same governance activity-recorder calls the graph nodes would have made. See `docs/governance/playbook-workflow-types.md` ("Repeatability: Rerun / Recall / Replay") for
the full mechanism shared across all three Playbooks.

## API Changes
- `POST api/v{version}/my/playbooks/taxonomy` (`RunMyTaxonomyPlaybookCommand`) executes the
  Playbook and persists a `PlaybookExecutionEntity` for the run. Accepts an optional `ReplayMode`
  (`Rerun`/`Recall`/`Replay`, default `Rerun`) and `SourceExecutionId` to recall or replay a prior
  execution instead of collecting fresh input.
- The catalog itself is queryable/creatable/updatable/deletable via the shared, unsecured
  `api/v{version}/playbooks` CRUD endpoints (`PlaybookCatalogEndpoints`): list, get by id, get by
  key, create, update general info, update one CER step, delete.

## Data Requirements
The catalog definition (`Playbooks` + `PlaybookSteps` tables) is seeded once at startup by
`PlaybookCatalogSeedInitializationService` with `Key = "taxonomy"`. Each run persists one row in
`PlaybookExecutions` (owner/tenant-scoped) via `SavePlaybookExecutionCommand`, recording
`CollectInput`, and the `CollectOutput`/`EvaluateOutput`/`RecordOutput` summary strings.

## Security Requirements
Any future API endpoint must apply the same tenant/owner scoping (`My`/`Our` request conventions)
as the rest of the application. Input text submitted for extraction should be treated as
potentially sensitive per `docs/governance/ai-policy.md` — adopters wiring a real UI/API surface
must apply the same secret/PII sanitation guidance documented there.

## Testing Requirements
- Integration: `Tests.Integration/Playbooks/TaxonomyPlaybookTests.cs` and
  `TaxonomyPlaybookStepDefinitions.cs` (`TaxonomyPlaybook.feature`) cover:
  - Tool discovery by attribute for all three stages.
  - End-to-end classification with queued model responses, asserting extracted terms, category,
    confidence, and exactly 3 governance records with non-deterministic replay metadata.
  - An unrecognized model-returned category throwing `InvalidOperationException`.

## Risks
- The five taxonomy categories are illustrative defaults; template adopters must replace
  `TaxonomyKnowledgeHolder.V1` with their own versioned categories before relying on this in
  production.
- No retry/repair loop exists for malformed model JSON today; a malformed response surfaces as an
  immediate failure.

## Out Of Scope
- Exposing this Playbook via a chat tool, REST endpoint, or UI page.
- Persisting results by default (persistence is opt-in via `SavePlaybookMaterializationCommand`).
- Automatic retry or self-correction of malformed model responses.

## Definition Of Done
- [x] Implementation complete (`Core.Application` + `Infrastructure.AgentFramework`).
- [x] Tests added/updated and passing (`Tests.Integration`, 136/136 solution-wide).
- [x] Build passes with 0 warnings/errors.
- [x] Documentation updated (this file, `docs/governance/playbook-workflow-types.md`, `README.md`).
- [x] Acceptance criteria satisfied.
