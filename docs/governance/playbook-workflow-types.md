# Playbook Workflow Types (Collect / Evaluate / Record)

## Purpose

`agent-framework-quick-start` ships three example Playbook workflows that all execute the
same Collect → Evaluate → Record (CER) stage contract shape from
`Goodtocode.Agents.Playbook`. The three examples exist to teach one idea: **CER is the
architectural abstraction; Microsoft Agent Framework (MAF) is just one orchestration
strategy capable of executing CER stage implementations.** The stage contracts, the
governance model, the resolver model, and (for the two MAF-backed examples) the workflow
graph shape all stay constant. The only thing that changes between examples is which stage
implementation is registered: deterministic or agentic.

| # | Example | Collect | Evaluate | Record | Orchestration |
|---|---------|---------|----------|--------|----------------|
| 1 | SQL Statistics Classification (`sql-statistics`) | Deterministic | Deterministic | Deterministic | `PlaybookExecutor` directly, no MAF |
| 2 | Taxonomy Extraction and Classification (`taxonomy`) | Agentic | Agentic | Agentic | MAF `WorkflowBuilder` / `InProcessExecution`, 3-node graph |
| 3 | Essay Rubric Evaluation (`essay`) | Deterministic | Agentic | Deterministic | Same MAF 3-node graph shape as #2; only the Evaluate tool differs |

Workflow 1 is a direct extension of the `document-review` example already in the
foundation (`DocumentReviewPlaybookDefinition`, `PlaybookExecutor<...>`); it proves
framework-agnostic CER execution with no LLM dependency.

Workflows 2 and 3 reuse one shared MAF graph builder (see below); workflow 3 is the
recommended real-world pattern — deterministic evidence retrieval and deterministic
materialization surrounding a single agentic reasoning stage.

## Project Boundaries

- **Core.Application** owns queries, commands, rubrics, knowledge contracts, evidence,
  finding, and materialization contracts, plus the `IPlaybookSteps<...>` playbook
  definitions. It must not depend on MAF, OpenAI, or any infrastructure implementation.
- **Infrastructure.AgentFramework** owns MAF workflow graph construction, agent-backed
  stage tools, chat client wiring, and all `Microsoft.Agents.AI*` usage.
- **Goodtocode.Agents.Playbook** (NuGet package) remains framework-agnostic: CER
  contracts, step interfaces, resolver abstractions, and the executor. No MAF or OpenAI
  dependency is introduced into it from this repository.

All three workflows are consumed purely through `PackageReference` to
`Goodtocode.Agents.Playbook` and `Goodtocode.Agents.Governance` — no `ProjectReference` to
the source of either package exists in this repository.

## MAF Adapter Placement Decision

**Decision:** keep MAF graph construction inside `Infrastructure.AgentFramework` in this
repository, as a host-owned adapter (`Execution/PlaybookWorkflowGraphBuilder`), rather than
extracting a new `Goodtocode.Agents.Playbook.AgentFramework` satellite package at this time.

**Rationale:**

- `Goodtocode.Agents.Playbook` remains permanently framework-agnostic; no direct MAF
  dependency is introduced into the shared package.
- Consumers of the shared package may adopt different orchestration technologies in the
  future without being forced to depend on MAF.
- CER contracts remain reusable outside MAF (as workflow 1 proves).
- This follows the dependency-inversion and governance boundaries already documented for
  this repository.

**Revisit when:** multiple repositories (for example `crucible-web` and this template)
begin duplicating near-identical MAF graph construction logic. At that point, consider
extracting `Goodtocode.Agents.Playbook.AgentFramework` as a satellite package that
references both `Goodtocode.Agents.Playbook` and Microsoft Agent Framework, while the base
package stays framework-independent.

## Shared MAF Graph Shape (Workflows 2 and 3)

Both MAF-backed workflows use one generic three-node graph, built once in
`Infrastructure.AgentFramework/Execution/PlaybookWorkflowGraphBuilder.cs` via
`Microsoft.Agents.AI.Workflows`'s `WorkflowBuilder` and run through `InProcessExecution`:

```
Collect -> Evaluate -> Record
```

Each node wraps a resolved Playbook step tool (`ICollectStepTool<,>`,
`IEvaluateStepTool<,>`, `IRecordStepTool<,>`) obtained from the same
`IPlaybookStepToolResolver<...>` used by the deterministic workflow. Swapping a
deterministic tool for an agentic one (or vice versa) for any one stage never changes the
graph shape — only the tool registered for that stage's resolver key changes. This mirrors
the pattern already proven in `crucible-web`'s `PipelineWorkflowStepExecutor`.

## Governance Expectations

Every stage execution of every workflow produces a real, validated
`EvaluationGovernanceRecord` (observability, auditability, defensibility, repeatability)
through a shared, generic `PlaybookGovernanceActivityRecorder<TEvidence, TFinding,
TMaterialization>` (see `Core.Application/Playbooks/PlaybookGovernanceActivityRecorder.cs`).
`Repeatability.DeterministicReplaySupported` is derived from the resolved tool's name
convention (`*.deterministic.*` vs `*.agentic.*`):

| Workflow | Collect replay | Evaluate replay | Record replay |
|----------|-----------------|------------------|-----------------|
| 1 — SQL Statistics | true | true | true |
| 2 — Taxonomy | false | false | false |
| 3 — Essay | true | false | true |

## Persistence: Playbook Catalog vs. Playbook Execution

The Playbook *concept* (what it is) and a Playbook *run* (how it executed, this one time) are
two separate, separately-persisted things:

- **`PlaybookEntity` / `PlaybookStepEntity`** (`Core.Domain/Playbooks`) are the semi-static,
  slow-moving **catalog**: one `PlaybookEntity` per workflow (`Key`, `Name`, `Description`,
  `WorkflowType`, `Version`) owning exactly three `PlaybookStepEntity` children, one per
  `PlaybookStepType` (`Collect`/`Evaluate`/`Record`). Each step persists its own
  `Name`/`Description` plus an `ActionFormat` (`SqlQuery`/`Rubric`/`Template`/`Prompt`) and
  `ActionDefinition` — the actual static query text, rubric/criteria description, or
  projection/prompt template that stage's tool runs. This is unsecured, shared reference data
  (`DomainEntity<T>`, not tenant/owner-scoped), queryable/creatable/updatable/deletable through
  the catalog CRUD endpoints under `api/v{version}/playbooks` (`PlaybookCatalogEndpoints`), and
  seeded once at startup for the three built-in workflows by
  `PlaybookCatalogSeedInitializationService`.
- **`PlaybookExecutionEntity`** (`Core.Domain/Playbooks`) is the per-run, tenant/owner-scoped
  (`SecuredEntity<T>`) record of one execution: a foreign key to the `PlaybookEntity` it ran
  against, `ReplayMode`/`SourceExecutionId` (repeatability metadata), and the dynamic
  `CollectInput`/`CollectOutput`/`EvaluateOutput`/`RecordOutput` strings produced by that run.
  Every `Run*PlaybookCommand` handler persists one `PlaybookExecutionEntity` via the shared
  `PlaybookExecutionPersister`/`SavePlaybookExecutionCommand`
  (`Core.Application/Playbooks/Persistence`) after the CER contract completes.

This separation means the same three questions always have distinct, independently queryable
answers: "what does this Playbook's Evaluate step always do?" (`PlaybookStepEntity.ActionDefinition`)
versus "what did this Playbook's Evaluate step produce the last time someone ran it?"
(`PlaybookExecutionEntity.EvaluateOutput`).

## Repeatability: Rerun / Recall / Replay

The Repeatability pillar of governance is made directly actionable through
`Goodtocode.Agents.Playbook.Execution.PlaybookReplayMode`, which every `Run*PlaybookCommand`
(`RunSqlStatisticsPlaybookCommand`, `RunTaxonomyPlaybookCommand`, `RunEssayPlaybookCommand`)
accepts alongside an optional `SourceExecutionId`:

| Mode | Collect | Evaluate | Record | Use case |
|------|---------|----------|--------|----------|
| `Rerun` (default) | Runs | Runs | Runs | Normal execution; also the only mode on a Playbook's first run. |
| `Recall` | Skipped | Skipped | Re-renders the prior Finding | "Show me exactly what happened last time," including after live data has since changed. |
| `Replay` | Skipped (reuses prior Evidence) | Re-runs | Runs | "Prove this Evaluate/Record logic reproduces the governed result from the same Evidence," without re-collecting from a (possibly now-different) live source. |

**How prior evidence/finding is persisted and resolved.** Every `Rerun` persists the stage's
typed `TEvidence` and `TFinding` as `PlaybookExecutionEntity.EvidenceJson`/`FindingJson`
(`System.Text.Json` serialized), in addition to the existing plain-string
`CollectOutput`/`EvaluateOutput`. When a request arrives with `ReplayMode` of `Recall` or
`Replay`, the handler calls the shared `PlaybookReplaySourceResolver`
(`Core.Application/Playbooks/Persistence`) to look up the prior `PlaybookExecutionEntity` — by
`SourceExecutionId` if supplied, otherwise the caller's most recent execution of that Playbook —
deserializes `EvidenceJson`/`FindingJson` back into `TEvidence`/`TFinding`, and constructs a
`PlaybookReplayContext<TEvidence, TFinding>` to pass to the runner. A prior execution that
predates this feature (no `EvidenceJson`/`FindingJson` persisted) cannot be recalled or replayed
and raises a `CustomConflictException`.

**Two different replay mechanisms, same contract.** The three example workflows reach
Recall/Replay through two different code paths, both driven by the same
`PlaybookReplayContext<TEvidence, TFinding>`:

- **SQL Statistics** (no MAF) calls the package's own replay-aware
  `PlaybookExecutor<...>.ExecuteAsync(definition, input, replayContext, ct, recorder)` overload
  directly — the package itself understands how to skip Collect/Evaluate against a supplied
  replay context.
- **Taxonomy and Essay** (MAF 3-node graph) use a repo-owned
  `PlaybookWorkflowGraphExecutor<...>.ExecuteAsync` that branches on `replayContext?.Mode` before
  touching the MAF graph at all: `Rerun` builds and runs the full `WorkflowBuilder` graph as
  before; `Recall`/`Replay` bypass the MAF graph entirely and call only the remaining stage
  tool(s) directly (`Recall` → `IRecordStepTool<,>.RecordAsync` against the prior Finding;
  `Replay` → `IEvaluateStepTool<,>.EvaluateAsync` then `IRecordStepTool<,>.RecordAsync` against
  the prior Evidence), while still invoking the same `IPlaybookStepActivityRecorder` calls the
  graph nodes would have made, so governance capture parity is preserved even though the MAF
  graph itself never runs for those two modes.

**UI surface.** Each of the 3 Playbook pages
(`Presentation.Web/Features/Playbooks/*PlaybookPage.razor`) shows "Rerun", "Recall", and "Replay"
controls (`PlaybookReplayControls.razor`) alongside the latest execution's result panel, each with
an inline explanation of what it does so the distinction is clear without reading this document.
Selecting any control re-invokes the same page's run command with the corresponding `ReplayMode`
and the latest execution's id as `SourceExecutionId` (ignored server-side for `Rerun`), using the
latest execution's own `CollectInput` rather than requiring the user to retype it. The result
panel re-renders in place with the rerun/recalled/replayed result — there is no before/after
comparison view by design, to keep the UI simple for a quick-start template.

## Unified Collect / Evaluate / Record Shape

All three example workflows deliberately follow one common input/criteria/output pattern, so a
developer who understands one workflow immediately understands the other two:

- **Collect input is always a plain `string`.** There is no workflow-specific request wrapper
  type at the playbook boundary (`TCollectInput` is `string` in all three `IPlaybookSteps<...>`
  definitions):
  - SQL Statistics: the target database name.
  - Taxonomy: the free-text prompt to extract taxonomy terms from.
  - Essay: the essay text itself.

  When the real-world source of that string is something richer (for example, an existing chat
  message), that resolution happens **outside** the playbook boundary, in the request handler that
  calls the runner (see `EvaluateEssayCommandHandler`), never inside a Collect-stage tool. This
  keeps every Collect-stage tool a simple, directly testable `string -> TEvidence` function.

- **Evaluation criteria are always supplied through the same type:** `Goodtocode.Agents.Playbook
  .Execution.PlaybookKnowledge` (instruction text, optional supporting items, and an
  `EvaluationRubric` made of weighted `EvaluationCriterion`s scored against either a
  `ContinuousEvaluationScale` or `DiscreteEvaluationScale`). Each workflow exposes its versioned
  criteria through a thin, per-workflow `IPlaybookKnowledgeHolder` singleton
  (`SqlStatisticsKnowledgeHolder.V1`, `TaxonomyKnowledgeHolder.V1`, `EssayKnowledgeHolder.V1`) so
  the holder type disambiguates DI registration while the underlying criteria payload type never
  changes between workflows. Only the criteria *data* differs per workflow:
  - SQL Statistics: a `ContinuousEvaluationScale` with Small/Medium/Large size bands (GB).
  - Taxonomy: a `DiscreteEvaluationScale` with five named category levels.
  - Essay: a single 0-1 `ContinuousEvaluationScale` applied to four independently weighted
    criteria (thesis clarity, evidence and support, organization, grammar and mechanics).

- **Record output is always reachable as a plain `string` summary**, in addition to whatever
  richer typed materialization each workflow also produces. Every Record-stage materialization
  implements the shared `IPlaybookMaterializationSummary` marker interface, so callers who just
  want "what happened, in one line" can call the shared extension method
  `result.Summary()` (`PlaybookExecutionResultExtensions.Summary<...>()`) instead of learning each
  workflow's specific materialization shape.

This is a deliberate low-complexity / high-transparency tradeoff for an open-source quick-start
template: the Collect and Record stages of the CER contract are easy to reason about in `string`
terms, while all genuinely structured work (tool calls, model prompts, rubric scoring,
classification, persistence shaping) stays inside the `IEvaluateStepTool<,>`/`IRecordStepTool<,>`
implementations, where it belongs.
