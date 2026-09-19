# Chat Journey Architecture

## Status and Scope

This document defines the architecture for **chat journeys**: ordered, suggested prompts and
context transitions that guide a user through a chain of related chat messages and product
operations.

A chat journey is similar to a customer journey through an ecosystem, but its unit of
progression is a chat-session message dialogue. The journey is intentionally thin: it helps a
user discover the next useful question, remembers selected context, and permits a user to enter
or resume the chain at any meaningful point.

This document governs:

- the concept and boundaries of a chat journey;
- suggested-prompt and action-option contracts;
- journey context and resumption;
- the relationship between journey guidance and intent routing;
- ambiguous or mid-journey user requests;
- implementation responsibilities in Application, Infrastructure, API, and UI;
- observability, privacy, and testing requirements.

Product-specific prompt wording, aggregate names, and tool catalogs belong in product
documentation and journey catalog contributors. This document is the reusable architecture.

## Problem Statement

A chat session is not merely a list of messages. A useful assistant helps the user move through
a sequence such as:

```text
orient
  -> identify the relevant actor or subject
  -> find or select a chat session
  -> inspect messages in that session
  -> narrow to a message, topic, time, or relationship
  -> ask a follow-up question about the selected evidence
```

The sequence is not always linear. Users can:

- start at the beginning with a broad request;
- select a result and continue one level deeper;
- ask a follow-up that assumes the current session;
- jump directly into the middle of the journey;
- ask an ambiguous question that requires one clarification;
- switch to another session and replace the active context.

The architecture must support all of these without making the user repeat identifiers that are
already known, and without silently selecting the wrong actor, session, or message.

## Core Concept

A **chat journey** is the combination of:

1. **Journey context** — the trusted, accumulated selection state for the current chat session.
2. **Journey definition** — the suggested prompts appropriate to that context.
3. **Journey step** — the current level, suggested prompts, and selectable action options.
4. **Intent route** — the typed operation selected when the user sends a prompt.
5. **Dialogue evidence** — the chat messages and tool results that explain what has already
   happened.

The journey is guidance, not an autonomous workflow. A suggested prompt is an invitation to
ask a question; it is not a command that executes merely because it is displayed.

```text
Chat session
    |
    +--> trusted active journey context
    |       (actor scope + selected subject/session/message context)
    |
    +--> journey provider
    |       -> suggested prompts for the current context
    |
    +--> user message or action-chip selection
            |
            v
        intent classification and routing
            |
            v
        typed application request/tool
            |
            v
        assistant response + selection options
            |
            v
        updated journey context and next journey step
```

## Architectural Principles

1. **The journey is a guidance layer, not a second router.** Intent classification and tool
   routing remain authoritative for deciding which operation executes.
2. **Context narrows scope; it does not manufacture certainty.** A selected session may scope
   “the last message,” but an unselected or ambiguous session must not be guessed.
3. **Every suggested prompt must be executable.** Its literal wording should resolve through the
   same intent catalog and typed tool path as an equivalent user-entered phrase.
4. **A user may enter at any level.** The system must support direct deep-link language, not only
   clicks through every preceding suggestion.
5. **Selections are explicit state transitions.** Selecting a session replaces or establishes
   active session context; it must not append stale identifiers from an unrelated journey.
6. **Responses expose the next useful action.** A tool result should help the user understand
   what can be asked next, while still allowing free-form questions.
7. **Ambiguity is observable and recoverable.** Ask for the smallest missing discriminator rather
   than selecting an arbitrary result.
8. **Journey state is compact and replayable.** Store stable identifiers and provenance, not an
   ever-growing replay of the entire conversation.
9. **Privacy follows the data.** Actor-scoped and session-scoped requests must enforce the same
   tenant, owner, and authorization rules as direct API requests.
10. **The latest dialogue is evidence, not authority.** A prior assistant sentence may suggest
   context, but only validated server-side context and typed identifiers establish scope.

## Journey Model

### Journey context

The current implementation represents the accumulated selection context with
`ChatJourneyContext`. It resolves a level from the deepest populated selection:

| Level | Meaning |
| --- | --- |
| `None` | No selected product context; show orientation prompts |
| `PipelineSelected` | A top-level pipeline or product definition is selected |
| `ExecutionSelected` | A run/instance is selected |
| `TimelineSelected` | A leaf-like timeline is selected |

For chat-specific journeys, the same model applies to a different resource chain:

```text
None
  -> actor identified
  -> chat session selected
  -> message or subject selected
  -> message-history question or follow-up
```

The exact enum values and fields may differ by product, but the invariant is the same:
`ResolveLevel()` must be deterministic from validated context, and the deepest valid selection
must determine the default prompt family.

A production chat-journey context should be capable of carrying, where relevant:

- authenticated actor and tenant scope;
- chat session identifier;
- selected subject/topic identifier or normalized subject key;
- selected message identifier;
- source of the selection (`explicit-id`, `action-chip`, `session-context`, or
  `validated-inference`);
- context version or timestamp;
- correlation identifier for the transition.

Do not persist raw secrets, credentials, or unnecessary message content in journey context.

### Journey definition

A `ChatJourneyDefinition` describes the prompts for one journey level and may be scoped to a
pipeline kit, product, tenant configuration, or another registered capability. Definitions are
catalog data, not business logic.

The catalog must:

- provide a default definition for the unselected state;
- prefer a more specific definition when context identifies a product or journey variant;
- return prompts in stable display order;
- keep prompts short, action-oriented, and suitable for literal routing;
- avoid exposing prompts whose prerequisites are not satisfied.

### Journey step

A `ChatJourneyStep` is the read model returned to the chat surface. It contains:

- the resolved level;
- suggested prompts;
- action options parsed from the latest assistant result.

The API DTO may add display-oriented fields, but it must not require the UI to reconstruct
journey state from markdown or from an unbounded message transcript.

## Prompt and Action Contracts

### Suggested prompts

Suggested prompts are a pure function of the current validated context and registered journey
definitions:

```text
suggestedPrompts = Provider.Resolve(context)
```

They should normally be limited to three or four actions. A useful set combines:

- the most likely next step in the active journey;
- one or two sibling or alternate views;
- a safe way to reset or broaden scope;
- an actor-scoped chat-history action when available.

For a chat-session journey, examples include:

```text
List my chat sessions
Show messages for this chat session
What was the last message about <subject>?
Find messages about <subject> in this session
```

The literal prompt text is part of the routing contract. Changing it may affect deterministic
phrase rules, semantic examples, evaluation data, and UI expectations. Prompt changes require
routing regression tests.

### Selection tokens

When a response presents selectable sessions, messages, actors, or subjects, it emits a stable
selection token alongside human-readable text:

```text
[selection|<kind>|<id>|<value>|<label>]
```

The token is a UI affordance, not the source of truth:

- `kind` identifies the selection type;
- `id` is the stable identifier used by a typed follow-up;
- `value` carries an optional canonical key or secondary value;
- `label` is the human-readable chip label.

The parser must ignore malformed tokens and preserve ordinary assistant prose. The action
option must build a literal selection prompt, with a generic fallback for new kinds. A click
must behave as if the user typed the resulting prompt.

### Next suggested action

Tool responses should state the next sensible action in ordinary language. This is useful when:

- the user does not use action chips;
- a client cannot render tokens;
- the journey is entered at a non-standard level;
- a result is empty and needs a recovery action.

The next action is guidance only. It must not imply that a future action will occur
automatically or proactively.

## Mid-Journey Entry and Resumption

### Direct entry is valid

Users may enter with a request that skips the normal list-and-select sequence. For example:

> “What was that last message about subject X?”

This may imply:

1. the user is referring to an existing chat-session journey;
2. the relevant actor is the authenticated actor;
3. the active session may already be known from the current chat context;
4. the user wants the latest message matching subject X, not a list of all sessions.

The system must resolve these implications in order of certainty:

```text
explicit session id
    -> validated active session context
    -> unique actor-scoped session match
    -> clarification when multiple sessions remain
```

If a validated active session exists, the request should route directly to a typed
session-scoped message query. It should not force the user through “list sessions” first.

If no active session exists and the actor has exactly one safe matching session, the system may
enter at the message level, but it must record that the session was resolved rather than
explicitly selected.

If multiple sessions could satisfy the request, ask a narrow clarification such as:

```text
Which chat session should I search: “Support discussion” from yesterday,
or “Project planning” from today?
```

Do not return the last message from an arbitrary session merely because it was most recently
updated. Recency may be a clarification hint, not an authorization to guess.

### Context precedence

When multiple sources provide context, use this precedence:

1. explicit, validated identifier in the current user message;
2. action-chip selection tied to the current assistant response;
3. validated active journey context for the current chat session;
4. a unique actor-scoped inference supported by the requested operation;
5. clarification.

A later explicit selection replaces an earlier selection at the same or higher level. A
selection from another chat session must never leak into the current session.

### Thin journeys

Chat-session journeys are intentionally thin because the session itself is already a dialogue
chain. The system should not model every conversational turn as a separate durable journey
node. Persist only the context needed to scope the next operation and explain how it was
resolved.

The durable chat message history remains the evidence. The journey context is a compact index
into that history.

## Relationship to Intent Classification and Routing

Journey resolution and intent routing are sequential but distinct:

```text
current journey context
    -> suggested prompt catalog
    -> user message
    -> intent classifier
    -> typed route/tool
    -> validated context transition
    -> refreshed journey step
```

The intent architecture remains responsible for:

- deterministic phrase and token rules;
- semantic classification when enabled;
- governed model fallback;
- typed argument binding;
- tool dispatch and application execution.

The journey architecture is responsible for:

- deciding which prompts are useful at the current level;
- carrying validated selected context;
- exposing action options;
- interpreting the result as a context transition;
- identifying when a direct request can safely enter in the middle.

Do not add journey-specific LLM classification when an existing intent route can express the
operation. Do not make an intent rule depend on UI-only prompt indexes. Shared concepts such as
“show messages for this session” should be represented in the intent catalog and referenced by
journey definitions.

## Layered Implementation

### Core.Application

Application owns the presentation-agnostic journey contracts and use cases:

- `ChatJourneyContext`;
- `ChatJourneyLevel`;
- `ChatJourneyDefinition`-equivalent contracts where shared;
- `IChatJourneyProvider`;
- `ChatJourneyStep` and DTO mapping;
- selection-token parsing;
- actor/session/message queries and authorization;
- context transition validation.

`GetChatJourneyStepQuery` should:

1. validate the authenticated user and chat-session identifier;
2. load the user-scoped session;
3. obtain the validated active context;
4. resolve the journey level and suggested prompts;
5. parse action options from the latest assistant/system response;
6. return a typed journey step.

Application must not contain embedding, model invocation, markdown formatting policy, or direct
UI component concerns.

### Infrastructure.AgentFramework

Infrastructure provides journey catalogs and providers:

- deterministic catalog lookup by level;
- kit/product-specific contributors;
- future semantic or hybrid providers behind `IChatJourneyProvider`;
- integration with agent context accessors;
- intent and tool routing.

The default deterministic provider should be the first tier. Semantic journey suggestions may be
added later, but the catalog remains the source of truth and semantic failure must fall back to
deterministic prompts.

### Presentation.Api

The API exposes a read-oriented journey-step endpoint scoped to the authenticated chat session.
It should return typed level, suggested prompts, and action options.

The endpoint must not accept an arbitrary actor or session context from the client without
authorization validation. Client-supplied selections are requests to resolve context, not proof
that the caller can access the referenced entity.

### Presentation.Web

The Blazor chat surface should:

- render suggested prompts as composer actions;
- render valid selection tokens as accessible action chips;
- submit chip prompts through the normal message path;
- refresh the journey step after session selection and assistant responses;
- preserve ordinary assistant text when token parsing fails;
- show clarification questions as normal assistant messages;
- avoid duplicating journey rules in component `if/else` cascades.

The UI should not infer session identity by scraping prose. It consumes the typed journey-step
DTO and the selection options returned by the API.

## Context Transition Rules

Every operation that can change journey context should return or emit a typed transition:

```csharp
public sealed record ChatJourneyContextTransition(
    ChatJourneyContext Context,
    string ResolutionSource,
    string? ResolutionReason);
```

The exact type may vary, but the transition should make clear:

- which context is now active;
- which identifier was selected or replaced;
- whether it came from explicit input, a chip, existing context, or inference;
- whether clarification is still required.

Examples:

| User action | Transition |
| --- | --- |
| Selects a chat session chip | Set active session; clear message-level selection |
| Asks for messages in the active session | Preserve session; move to message-history level |
| Selects a message | Set active message; preserve session |
| Starts a new chat session | Clear prior journey context |
| Explicitly selects another session | Replace active session and clear dependent message/subject context |
| Ambiguous “last message” request | No transition; ask for session discriminator |

Context transitions must be idempotent. Re-selecting the active session must not duplicate
state or create a second logical journey branch.

## Observability and Privacy

Journey telemetry should record privacy-safe metadata:

- chat-session identifier and correlation identifier;
- previous and resulting journey level;
- resolution source;
- selected resource kinds, stable identifiers, or hashes as policy permits;
- suggested-prompt catalog/version;
- action-token parse count and malformed-token count;
- route outcome: resolved, clarified, unauthorized, not found, or failed;
- latency and provider tier.

Do not log raw message content, subject text, captured identifiers, or assistant responses by
default. If message content is required for approved diagnostics, apply the AI policy's
redaction, retention, and authorization requirements.

Journey context must enforce the same `OwnerId` and `TenantId` rules as the underlying chat
queries. “My chat sessions” and “messages for this session” are actor-scoped operations, not
global searches.

## Failure and Ambiguity Handling

The system should distinguish:

- **No context:** offer orientation prompts.
- **Context not found:** clear invalid dependent context and explain the recovery action.
- **Unauthorized context:** return the standard authorization/problem response; do not reveal
  whether another actor's session exists.
- **Multiple matches:** ask a narrow clarification with safe, non-sensitive labels.
- **No matching messages:** retain the session context and suggest a broader subject/time query.
- **Malformed action token:** ignore the token and show the assistant response as prose.
- **Journey provider failure:** use the deterministic default catalog and emit observable failure
  telemetry.
- **Intent route miss:** continue through the normal routing fallback; do not invent a journey
  transition.

A journey provider should not return a success-shaped empty response when it failed to resolve
the catalog. If no definitions exist at all, the failure must be explicit in telemetry and the
API should return the repository-standard error or a documented safe fallback.

## Crucible Product Instantiation

Crucible currently instantiates this architecture as a progressive journey through pipeline
runtime evidence:

```text
Pipeline Kits / Pipelines
    -> selected Pipeline
    -> selected Execution
    -> Chronicle or Timelines (parallel branches)
    -> selected Timeline
    -> Timeline Events
```

The journey is intentionally not a forced linear sequence after an execution is selected.
Chronicle and Timelines are co-equal views of the same execution:

- **Chronicle** provides ordered narrative sections for an explainable historical or audit view.
- **Timelines** provide CER-aware operational event groups and can be selected independently.
- **Timeline Events** provide bounded, ordered events scoped to the selected timeline code.

This supports the primary Crucible actors:

- **Operator** — asks what is running, what happened, and what is next; typically enters through
  the latest pipeline or timeline snapshot.
- **Auditor/Reviewer** — asks what happened, why, and what evidence supports it; typically enters
  through the Chronicle branch.
- **Platform Administrator and Solution Builder** — use the same journey to diagnose failures
  and validate pipeline narrative and timeline output.

### Crucible journey levels

| Product level | Context transition | Typical next prompts |
| --- | --- | --- |
| No selection | Orient the user and list pipelines | Query pipelines; inspect profile; learn the journey; list my chat sessions |
| Pipeline selected | Set pipeline; clear execution and timeline context from another pipeline | List executions; inspect scope or plan; execute the selected pipeline |
| Execution selected | Set execution while retaining its pipeline | Query Chronicle; query Timelines; inspect timeline events; inspect the plan |
| Chronicle/Timelines branch | Read either branch from the same execution | Cross-reference the other branch or select a timeline |
| Timeline selected | Set timeline code and preserve its execution | Query timeline events; return to Chronicle; run the selected pipeline |

The exact tool names remain implementation details, but the product journey must preserve these
semantic transitions. Direct lookup by pipeline identifier or name may bypass the browse step,
but it must establish the same validated active pipeline context as chip selection.

### Product-specific cross-cutting actions

Crucible offers these actions independently of the pipeline hierarchy:

- **Actor profile lookup** uses the authenticated actor scope and is available as an orientation
  action.
- **List my chat sessions** uses the authenticated `OwnerId` and `TenantId` scope and does not
  require a selected pipeline, execution, or timeline.
- **Show messages for this chat session** resolves the active chat-session identifier and uses
  the same actor scope. It is safe to offer at every pipeline journey level because it does not
  mutate pipeline, execution, or timeline context.
- **Execute the selected pipeline** requires explicit `confirmed=true`. It is a single-turn
  status operation; the response must tell the user to send a new message to check status rather
  than imply proactive follow-up.

These are actor-scoped detours, not additional nodes in the pipeline hierarchy. A user at the
Timeline level can inspect their chat messages and then return to the same pipeline context.

### Current implementation and known gaps

The Crucible implementation uses a deterministic journey catalog and provider, with
`ChatJourneyContext` resolving the deepest selected level and `GetChatJourneyStepQuery`
returning suggested prompts plus action options parsed from the latest assistant/system reply.
Selection tokens provide clickable pipeline, execution, and timeline transitions while retaining
plain assistant prose as the human-readable source.

The current product behavior is intentionally foundational:

- suggested prompts are static per level rather than adaptive to execution outcome, anomalies,
  or CER threshold variance;
- Chronicle and Timelines are conceptually parallel but are requested as separate chat actions,
  not rendered side-by-side in one response;
- prompt sets do not yet vary by Operator versus Auditor role;
- direct mid-journey chat-session requests require active-context or safe actor-scoped
  resolution, with clarification when multiple sessions match.

These are product evolution opportunities, not reasons to weaken the universal journey
contracts.

## Testing and Change Management

### Journey-provider tests

- each level resolves the expected prompt set;
- kit/product-specific definitions override general definitions;
- missing specific definitions fall back to general definitions;
- provider cancellation and catalog failure are observable;
- prompts remain literal routes recognized by the intent catalog.

### Context tests

- deepest valid selection determines the level;
- selecting a session clears dependent message context;
- starting a new session clears prior context;
- re-selecting an active item is idempotent;
- context never crosses chat-session or actor boundaries;
- explicit identifiers take precedence over inferred context;
- ambiguous sessions produce clarification rather than arbitrary selection.

### Token and UI tests

- valid tokens produce typed action options;
- malformed tokens do not remove ordinary prose;
- unknown selection kinds use the generic prompt fallback;
- chip submission reaches the same route as typed selection text;
- the UI renders typed options accessibly and refreshes after a transition.

### End-to-end journey tests

At minimum, test these flows through supported APIs and the real chat pipeline:

1. list actor-scoped chat sessions, select one, and show its messages;
2. start with an active session and ask for the last message about a subject;
3. start without a session and verify an ambiguous request asks for clarification;
4. start without a session where exactly one safe match exists and verify direct mid-journey
   entry;
5. switch sessions and verify the previous session's context cannot affect the new request;
6. select a message and ask a scoped follow-up;
7. verify unauthorized session identifiers do not disclose existence.

Changes to prompt wording, journey levels, selection-token grammar, or context fields are
compatibility changes. Update the intent catalog, providers, API contracts, UI consumers, and
tests together.

## Related Documents and Implementation References

- [Intent classification and routing architecture](architecture-intent-classification-and-routing.md)
- [Progressive selection chat design](design-ux-progressive-selection-chat.md)
- [AI policy](ai-policy.md)
- [Tool design and implementation](tool-design-and-implementation.md)
- [Chat journey context](../../src/Core.Application/Chats/Journeys/ChatJourneyContext.cs)
- [Chat journey provider](../../src/Core.Application/Chats/Journeys/IChatJourneyProvider.cs)
- [Chat journey step query](../../src/Core.Application/Chats/GetChatJourneyStepQuery.cs)
- [Journey definitions](../../src/Infrastructure.AgentFramework/Journeys/ChatJourneyDefinition.cs)
- [Deterministic journey provider](../../src/Infrastructure.AgentFramework/Journeys/RuleChatJourneyProvider.cs)
- [Journey catalog](../../src/Infrastructure.AgentFramework/Journeys/ChatJourneyCatalog.cs)
- [Selection-token parser](../../src/Core.Application/Chats/Journeys/ChatSelectionTokenParser.cs)
- [Crucible chat journey feature](../product/features/feature-chat-journey.md)
