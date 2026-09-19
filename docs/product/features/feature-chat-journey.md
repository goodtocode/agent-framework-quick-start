# Feature: Pipeline Workspace Chat Journey

## Status
This document describes an already-implemented customer/user journey in Crucible-specific terms:
using the AI agent chat surface to progressively browse Pipeline Kits → Pipelines → Executions →
(Chronicle + Timelines, viewed in parallel) → Timeline Events. It is the Crucible instantiation of
two universal governance patterns and should be read alongside them, not instead of them:

- `docs/governance/design-ux-progressive-selection-chat.md` — the AI agent chat protocol pattern
  (selection tokens, context accumulation, suggested prompts).
- `docs/governance/design-ux-progressive-selection-UI.md` — the page/layout pattern that hosts the
  chat surface.
- `docs/governance/design-Intent-classification-and-routing.md` — how a typed/clicked phrase
  resolves to the correct tool call in the first place.

This journey is intentionally documented as it exists today: a static, level-by-level guided
descent. It is not yet dynamic (e.g., it does not yet adapt prompts to execution outcome, health,
or anomaly signals). Known opportunities for improvement are called out at the end of this
document rather than treated as blockers to documenting the foundational journey.

Read this product-specific feature together with
[`docs/governance/architecture-chat-journey.md`](../../governance/architecture-chat-journey.md),
which defines the reusable journey contracts, context precedence, mid-journey entry, and
implementation boundaries.

## Journey Summary

A user lands on `/chat`, and without needing to know any identifiers, can:

1. **List Pipeline Kits/Pipelines** — "Query our pipeline list in Censic Crucible."
2. **Select a Pipeline** — click a `pipeline` chip (or type "Select pipeline `<id>`").
3. **List Executions for that Pipeline** — the assistant now implicitly scopes to the selected
   pipeline.
4. **Select an Execution** — click an `execution` chip.
5. **View Chronicle and Timelines in parallel** — from a selected execution, the user can
   independently ask for the narrative Chronicle (sections) or the Timelines (groups), without
   either blocking the other; both are framed as co-equal next steps, not a forced sequence.
6. **Select a Timeline** — click a `timeline` chip surfaced from the timelines list (or from the
   latest-snapshot view).
7. **List Timeline Events for that Timeline** — the assistant scopes strictly to the selected
   timeline code, returning a bounded, ordered event table.

At every step the assistant's reply ends with an explicit "next suggested action," and the
suggested-prompt strip beneath the composer updates to reflect the deepest level currently
selected.

## Journey Actors

Per `docs/product/sprint-0/crucible-ux-journeys.md`, the primary actors for this chat journey are
the **Operator** (day-to-day: "what's running, what happened, what's next") and the **Auditor /
Reviewer** (historical: "what happened, why, what evidence"). The **Platform Administrator** and
**Solution Builder** use this same journey when triaging failures or validating a pipeline's
narrative/timeline output during iteration.

## Level-by-Level Detail

### Level 0 — No Selection (Orientation)
- Tool: `QueryOurPipelineListAsync` ("list pipelines", "show our pipelines", "browse pipelines").
- Returns a markdown table of tenant pipelines plus one `[selection|pipeline|...]` token per row.
- Suggested prompts (no context selected):
  - "Query our pipeline list in Censic Crucible"
  - "Query my actor user profile details by current authenticated user scope"
  - "Show how to select a pipeline and inspect executions, chronicles, and timelines"
  - "List my chat sessions"
  - "Show messages for this chat session"
- A "latest pipeline snapshot" shortcut also exists (`GetLatestPipelineSnapshot`-style tool) that
  reports the latest pipeline/execution plus chronicle section count, timeline group count, and
  timeline event count in one reply, with timeline chips for immediate drill-down — this is the
  fast path for an Operator who just wants "what's the current state."

### Level 1 — Pipeline Selected
- Selecting a `pipeline` chip calls `SetActivePipelineContextAsync`, which stores the pipeline id
  (and CURI) as active chat context. No execution/timeline context carries over from a prior
  pipeline.
- Suggested prompts:
  - "Query our execution list within the selected pipeline"
  - "Query our pipeline scope"
  - "Query our pipeline plan"
  - "Execute and run the selected pipeline (confirmed=true)"
  - "List my chat sessions"
  - "Show messages for this chat session"
- A pipeline can also be looked up directly by id or name (`GetPipelineByIdAsync`,
  `GetPipelineByNameAsync`) for users who already know what they're after, bypassing the browse
  step but still setting the same active context.

### Level 2 — Execution Selected
- `GetExecutions` lists executions for the active (or explicit) pipeline; each row carries an
  `execution` selection token.
- Selecting an `execution` chip calls `SetActiveExecutionContextAsync`.
- Suggested prompts:
  - "Query our chronicle list from the selected execution"
  - "Query our timelines list from the selected execution"
  - "Query our timeline events list from the selected timeline"
  - "Query our pipeline plan"
  - "List my chat sessions"
  - "Show messages for this chat session"
- This is the level where Chronicle and Timelines branch **in parallel** — see next section.

### Level 3 — Chronicle and Timelines (Parallel Branches)
Both branches read from the same selected execution and are independently selectable; neither
requires visiting the other first:

- **Chronicle branch**: `QueryOurChronicleListFromExecutionAsync` / `GetChronicleById` return
  ordered narrative sections for the execution. The reply's next suggested action points back to
  timelines/timeline events, reinforcing that the two branches are meant to be cross-referenced,
  not walked linearly.
- **Timelines branch**: `QueryOurTimelinesListFromExecutionAsync`-style tool returns ordered
  timeline groups, each carrying a `timeline` selection token (`[selection|timeline|<code>|...]`).
  Its next suggested action points to timeline events for the selected timeline.

This parallel framing matches the Crucible value flow (`Scope → Plan → Runtime → Actions →
Statements` in `docs/product/sprint-0/crucible-ux-journeys.md`): Chronicle is the explainable
narrative/statement view of a run; Timelines is the CER-aware, event-level operational view of the
same run. An Auditor typically starts from Chronicle; an Operator typically starts from Timelines —
the journey supports both entry points equally.

### Level 4 — Timeline Selected → Timeline Events
- Selecting a `timeline` chip calls `SetActiveTimelineContextAsync(timelineCode)`.
- `QueryOurTimelineEventsListFromTimelineAsync` returns a bounded, ordered event table scoped to
  that timeline code (falling back to the active timeline context when no code is passed
  explicitly).
- Suggested prompts at this level:
  - "Query our timeline events list for timeline code `<code>`"
  - "Query our chronicle list from the selected execution"
  - "Execute and run the selected pipeline (confirmed=true)"
  - "List my chat sessions"
  - "Show messages for this chat session"
- This is the leaf level of the journey: individual CER-relevant runtime events, the same event
  granularity described in `docs/product/features/feature-timeline.md` and
  `docs/product/features/feature-chronicle.md`.

## Cross-Cutting Actions

- **Run a pipeline**: available at any level once a pipeline is in context
  (`ExecuteAndRunPipelineAndReturnPipelineUrlAsync`), gated behind an explicit `confirmed=true`.
  The reply is a single-turn status report; per the chat governance doc's stateless-turn
  principle, it explicitly tells the user to send a new message to check status rather than
  implying it will follow up proactively.
- **Actor profile lookup**: "Query my actor user profile details by current authenticated user
  scope" is always offered at Level 0 as an orientation/identity-check action independent of the
  pipeline hierarchy.
- **Actor-scoped session jumps** (jump straight or midway into/out of the standard aggregate
  chain): "List my chat sessions" and "Show messages for this chat session" are offered at
  **every** level (0-4), not just Level 0. Both resolve deterministically from the current actor's
  claims-derived owner/tenant scope, with no pipeline/execution/timeline context required:
  - "List my chat sessions" → `GetMyChatSessionsQuery`, filtered by the authenticated user's
    `OwnerId`/`TenantId` (from claims, e.g. `oid`/`objectid`) — independent of any selected
    pipeline, execution, or timeline.
  - "Show messages for this chat session" → resolves the *currently active* chat session id
    automatically (no GUID typing or selection step) and returns that session's messages, filtered
    by the same `OwnerId`/`TenantId` scope, via `GetMyChatSessionMessagesQuery`.
  - Because these are pure actor+active-session lookups, a user mid-journey (e.g. at Level 3,
    Timeline selected) can jump to either without losing their existing pipeline/execution/timeline
    context — the underlying `ChatToolSessionContext` is untouched by either action.

## Where This Journey Is Implemented

- Chat protocol/tooling: `src/Infrastructure.AgentFramework/Tools/PipelineWorkspaceTool.cs`,
  `src/Infrastructure.AgentFramework/Tools/MyChatSessionsTool.cs`,
  `src/Infrastructure.AgentFramework/Tools/MyChatMessagesTool.cs`,
  `src/Infrastructure.AgentFramework/ChatMessageIntentRouter.cs`,
  `src/Infrastructure.AgentFramework/Journeys/DefaultChatJourneyCatalogContributor.cs` (Tier 1a
  suggested-prompt catalog, including the actor-scoped session jumps above).
- Chat sessions/messages queries: `src/Core.Application/Chats/GetMyChatSessionsQuery.cs`,
  `src/Core.Application/Chats/GetMyChatSessionMessagesQuery.cs`.
- Page/UI shell: `src/Presentation.Web/Features/Chats/ChatPage.razor`, with supporting components
  in `src/Presentation.Web/Features/Chats/Components/` (`ChatActionStrip.razor`,
  `SuggestedPromptsStrip.razor`, `ChatMessageList.razor`, `ChatSessionList.razor`,
  `ChatSessionStrip.razor`, `NewChatMessageInput.razor`, `NewChatMessageCard.razor`).
- Selection model: `src/Core.Application/Chats/Journeys/ChatJourneyActionOption.cs` (Application
  layer), mapped to `src/Presentation.Web/Features/Chats/Models/ChatSelectionOption.cs` for the web
  layer.

Related domain features: `docs/product/features/feature-chronicle.md`,
`docs/product/features/feature-timeline.md`, `docs/product/features/feature-execution.md`.

## Known Gaps / Future Improvements (Not Blocking This Documentation)

- The journey is currently **static**: suggested prompts and next-action text are fixed strings
  per level, not adapted to execution outcome (success/failure), anomaly signals, or CER
  threshold/variance state.
- Chronicle and Timelines are documented as parallel branches conceptually, but the chat surface
  does not yet render them side-by-side in a single reply — a user must ask for one, then the
  other.
- Suggested prompts do not yet vary by actor role (Operator vs. Auditor), though the underlying
  tools already support both usage patterns.
- These gaps are tracked for future iteration and do not change the foundational journey
  principles documented above.
