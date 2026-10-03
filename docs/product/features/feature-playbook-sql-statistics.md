# Feature: Playbook — SQL Statistics Classification

## Overview
A fully deterministic example Playbook that classifies a SQL Server database as Small, Medium, or
Large by total size in GB. It is the simplest of the three example Playbooks shipped in this
quick-start: no LLM is involved anywhere in its Collect → Evaluate → Record (CER) pipeline. It
exists to prove that CER is a framework-agnostic architectural abstraction — Microsoft Agent
Framework (MAF) is one possible orchestration strategy for CER stages, not a requirement of the
pattern itself.

## Business Problem
Teams adopting an agentic application framework often assume every capability must involve a
model call. That assumption adds unnecessary cost, latency, and non-determinism to capabilities
that are naturally rule-based (size thresholds, lookups, deterministic transforms). Developers
need a concrete, working example of a governed capability that needs no model at all, so they
don't over-apply agentic patterns where a deterministic rule suffices.

## Business Value
- Demonstrates the lowest-cost, highest-transparency point on the CER spectrum: fully replayable,
  fully deterministic, no token spend.
- Gives template adopters a direct, copyable pattern for wrapping any existing deterministic
  business rule (size bands, quota checks, status derivations, etc.) in the same governed CER
  shape used by the agentic examples, so capabilities can be mixed and matched by evidence need,
  not forced into one architecture.
- Establishes the baseline against which the agentic examples
  ([Taxonomy](./feature-playbook-taxonomy.md), [Essay](./feature-playbook-essay.md)) are
  contrasted.

## User Story
As a developer evaluating this quick-start template, I want a working example of a fully
deterministic governed Playbook, so that I understand which capabilities don't need an LLM and how
to build them with the same governance guarantees as agentic ones.

## Related Ontology Concepts
- **Governed Inference** (`docs/product/sprint-0/ontology.md`) — every CER stage execution, agentic
  or not, produces a governance record.
- **Tool** — each CER stage (Collect/Evaluate/Record) is implemented as a typed, scoped tool.
- See `docs/governance/playbook-workflow-types.md` for the full three-workflow comparison and the
  shared "Unified Collect / Evaluate / Record Shape" convention this feature follows.

## User Flow
Not a chat-driven user flow. This Playbook is exercised programmatically through
`ISqlStatisticsClassificationRunner` (or directly via `PlaybookExecutor<...>` and
`SqlStatisticsPlaybookDefinition`) and is currently proven through integration tests rather than an
exposed API/UI surface. See [API Changes](#api-changes) for what adding an endpoint would require.

## Acceptance Criteria
- [x] Collect stage retrieves the current size, table count, and index count for a named database,
      with no LLM involvement.
- [x] Evaluate stage classifies the collected size against a versioned, named-band rubric with no
      LLM involvement.
- [x] Record stage produces a materialization carrying both the typed classification and a
      plain-text summary.
- [x] Every stage execution produces a valid `EvaluationGovernanceRecord` with
      `Repeatability.DeterministicReplaySupported == true` for all three stages.
- [x] The Collect-stage input is a plain `string` (database name) — the same shape convention used
      by the Taxonomy and Essay Playbooks.

## Functional Requirements
- Collect-stage input is the database name as a plain `string`.
- Database size, table count, and index count are retrieved via `ISqlDatabaseStatisticsProvider`
  and returned as `SqlDatabaseStatisticsEvidence`.
- Classification bands are defined once, as versioned data, not inline conditionals scattered
  across code.
- The Record-stage materialization must remain summarizable as a single string via
  `result.Summary()` regardless of its richer typed shape.

## Domain Changes
This Playbook's catalog definition is persisted as a `PlaybookEntity` (`Key: sql-statistics`)
with three `PlaybookStepEntity` children (`Collect`/`Evaluate`/`Record`), each carrying the
step's `ActionFormat` and static `ActionDefinition` (the T-SQL statistics query, the size-band
rubric text, and the materialization/summary template, respectively). Every run is persisted as
a `PlaybookExecutionEntity` — see [Data Requirements](#data-requirements) and
`docs/governance/playbook-workflow-types.md` ("Persistence: Playbook Catalog vs. Playbook
Execution") for the full entity model shared by all three example Playbooks.

## Application Changes
All types live in `Core.Application/Playbooks/SqlStatistics/`:

- `SqlDatabaseStatisticsEvidence(string DatabaseName, double SizeGb, int TableCount, int IndexCount, DateTimeOffset CollectedUtc)`
  — Collect-stage output.
- `SqlStatisticsKnowledgeHolder` — wraps a `Goodtocode.Agents.Playbook.Execution.PlaybookKnowledge`
  built from a `ContinuousEvaluationScale` with three named entries: `Small` (0–20 GB), `Medium`
  (20–100 GB), `Large` (100 GB+, inclusive at the lower bound of each band). This is the same
  `PlaybookKnowledge`/`EvaluationRubric` type every example Playbook uses to carry its criteria —
  see `docs/governance/playbook-workflow-types.md`.
- `SqlDatabaseSizeFinding(string DatabaseName, double SizeGb, SqlDatabaseSizeClassification Classification, string Reason, string RubricVersion)`
  — Evaluate-stage output.
- `SqlDatabaseSizeMaterialization(string DatabaseName, SqlDatabaseSizeClassification Classification, double SizeGb, string Summary)`
  — Record-stage output; implements `IPlaybookMaterializationSummary`.
- `SqlStatisticsPlaybookDefinition : IPlaybookSteps<string, SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>`
  — the CER contract definition.
- `ISqlStatisticsClassificationRunner` / `SqlStatisticsClassificationRunner` — the framework-facing
  entry point: `ClassifyAsync(string databaseName, CancellationToken) -> Task<PlaybookExecutionResult<...>>`,
  driving a plain `PlaybookExecutor<...>` directly (no MAF dependency at all).

## Agent and Tool Contract
No `AIAgent` is involved. The three stage tools, registered in
`Infrastructure.AgentFramework/Playbooks/SqlStatistics/` and discovered by
`[PlaybookTool("...")]` attribute via `AddSqlStatisticsPlaybookTools()`:

| Stage | Tool name | Implementation |
|-------|-----------|-----------------|
| Collect | `sql-statistics.deterministic.collect` | `SqlStatisticsCollectTool` — builds `SqlDatabaseStatisticsQuery` from the input string and dispatches it through `IToolApplicationExecutor`. |
| Evaluate | `sql-statistics.deterministic.evaluate` | `SqlStatisticsEvaluateTool` — orders the rubric's `ContinuousEvaluationScale.Entries` by `Maximum` and selects the first band the evidence's `SizeGb` falls within. |
| Record | `sql-statistics.deterministic.record` | Shapes the finding into `SqlDatabaseSizeMaterialization`, including the plain-text `Summary`. |

No tool here is LLM-selectable; the workflow (not a model) chooses which operation runs at each
stage, matching the "deterministic" classification in the workflow-type comparison table.

## Prompt, Model, and Memory
Not applicable — no model call occurs anywhere in this Playbook.

## Observability and Audit
Every stage execution is recorded by `SqlStatisticsGovernanceActivityRecorder` (a
`PlaybookGovernanceActivityRecorder<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>`),
producing a full `EvaluationGovernanceRecord` (observability, auditability, defensibility,
repeatability) per stage, per the four governance pillars in
`docs/governance/ai-policy.md`. Because every stage tool name matches the `*.deterministic.*`
naming convention, `Repeatability.DeterministicReplaySupported` is `true` for all three stages.

## Runtime States
- **Successful**: all three stages complete; result carries evidence, finding, and materialization.
- **Failed**: the named database does not exist, or `ISqlDatabaseStatisticsProvider` throws —
  surfaces as a normal exception from `ClassifyAsync`.
- Not applicable: streaming, pending/loading, or unauthorized states — this Playbook has no chat/UI
  surface today (see [User Flow](#user-flow)).

## Evaluation and Quality Criteria
Not applicable in the LLM-evaluation sense (no model output to score). Correctness is validated by
deterministic boundary tests (see [Testing Requirements](#testing-requirements)): size exactly at a
band boundary classifies into the lower band (inclusive), confirming the rubric's `Maximum` values
are inclusive upper bounds.

## UI Changes
The `SqlStatisticsPlaybookPage` (`Presentation.Web/Features/Playbooks`) runs this Playbook from a
database-name input, then renders the shared `PlaybookResultPanel`: a Collect/Evaluate/Record
toggle, a summary card, and a step-detail panel. The step-detail panel fetches this Playbook's
catalog entry (`GetPlaybookByKeyAsync("sql-statistics")`) to show each step's persisted
`Description` and `ActionDefinition` (the actual statistics query) alongside that run's dynamic
input/output.

## API Changes
- `POST api/v{version}/my/playbooks/sql-statistics` (`RunMySqlStatisticsPlaybookCommand`) executes
  the Playbook and persists a `PlaybookExecutionEntity` for the run.
- The catalog itself is queryable/creatable/updatable/deletable via the shared, unsecured
  `api/v{version}/playbooks` CRUD endpoints (`PlaybookCatalogEndpoints`): list, get by id, get by
  key, create, update general info, update one CER step, delete.

## Data Requirements
The catalog definition (`Playbooks` + `PlaybookSteps` tables) is seeded once at startup by
`PlaybookCatalogSeedInitializationService` with `Key = "sql-statistics"`. Each run persists one
row in `PlaybookExecutions` (owner/tenant-scoped) via `SavePlaybookExecutionCommand`, recording
`CollectInput`, and the `CollectOutput`/`EvaluateOutput`/`RecordOutput` summary strings.

## Security Requirements
Any future API endpoint must apply the same tenant/owner scoping (`My`/`Our` request conventions)
as the rest of the application. No new sensitive-data classes are introduced; database names and
size metrics are operational metadata, not regulated or personal data.

## Testing Requirements
- Integration: `Tests.Integration/Playbooks/SqlStatisticsPlaybookTests.cs` and
  `SqlStatisticsPlaybookStepDefinitions.cs` (`SqlStatisticsPlaybook.feature`) cover:
  - Tool discovery by attribute for all three stages.
  - Boundary classification across `[DataRow]`s (5.0→Small, 20.0→Small, 20.1→Medium, 100.0→Medium,
    100.1→Large).
  - Exactly 3 governance records produced per execution, each with a non-empty `TraceId`,
    `PromptHash`, and `InputHash`.

## Risks
- Hardcoded size bands (20 GB / 100 GB) are illustrative defaults, not tuned for any real
  environment; template adopters must replace `SqlStatisticsKnowledgeHolder.V1` with their own
  versioned bands before relying on this in production.

## Out Of Scope
- Exposing this Playbook via a chat tool, REST endpoint, or UI page.
- Persisting results by default (persistence is opt-in via `SavePlaybookMaterializationCommand`).
- Any LLM-driven reasoning — that is intentionally the role of the other two example Playbooks.

## Definition Of Done
- [x] Implementation complete (`Core.Application` + `Infrastructure.AgentFramework`).
- [x] Tests added/updated and passing (`Tests.Integration`, 136/136 solution-wide).
- [x] Build passes with 0 warnings/errors.
- [x] Documentation updated (this file, `docs/governance/playbook-workflow-types.md`, `README.md`).
- [x] Acceptance criteria satisfied.
