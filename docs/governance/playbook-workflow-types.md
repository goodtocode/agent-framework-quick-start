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

## Optional Persistence

All three workflows may optionally persist their typed materialization through a shared
`SavePlaybookMaterializationCommand` (`Core.Application/Playbooks/Persistence`) backed by a
single `PlaybookMaterializations` table, so no workflow needs a bespoke persistence schema.
