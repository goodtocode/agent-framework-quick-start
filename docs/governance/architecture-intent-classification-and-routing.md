# Chat Message Intent Classification and Routing Architecture

## Status and Scope

This document is the architectural standard for routing a user's free-form chat message to the
correct product or web-search tool in Microsoft Agent Framework (MAF) /
Microsoft.Extensions.AI (MEAI) applications built from this template.

The system is deliberately **tool-first**. A known product capability or web-search request should
resolve to its tool before the model is asked to produce a general conversational answer. The
model remains essential for long-tail wording, typed argument binding, and prompts that genuinely
do not map to a tool, but it is not the primary routing mechanism.

This document governs the routing mechanisms, their order, and their architectural boundaries.
Repository-specific intent catalogs, phrase examples, thresholds, configuration values, and rollout
evidence belong in the relevant product documentation.

## Design Principles

1. **Prefer the least expensive reliable mechanism.** Use deterministic classification before
   vector similarity, and vector similarity before an LLM turn.
2. **Route tools before conversational answers.** A message that applies to a registered tool
   should invoke that tool rather than receive an announcement that the assistant will act later.
3. **Keep dispatch deterministic after classification.** Levels 1 and 2 resolve to the same
   catalog intent and deterministic route; only Level 3 delegates routing to the agent runtime.
4. **Preserve architecture boundaries.** Classification, response shaping, and agent fallback are
   infrastructure/presentation-adjacent concerns. Application and domain requests remain typed,
   presentation-agnostic use cases.
5. **Treat ambiguity as a miss.** Do not choose an arbitrary deterministic intent when multiple
   rules have equal specificity. Fall through to the next level.
6. **Govern every inference.** Embedding and LLM calls must comply with the repository governance
   and AI-policy standards. Deterministic-only routing does not create a model inference.

## Three-Level Routing Model

```text
User message
    |
    v
Level 1: Deterministic classification and routing
    |
    +--> Match ----------------------------------------> deterministic route and tool result
    |
    +--> Miss
            |
            v
Level 2: Embedding and vector-search classification
    |
    +--> Confident match ------------------------------> deterministic route and tool result
    |
    +--> Miss, disabled, low confidence, or failure
            |
            v
Level 3: Governed LLM routing and conversational fallback
    |
    +--> Forced tool selection ------------------------> tool result when a tool is invoked
    |
    +--> No usable forced-tool result -----------------> open agent turn
```

The levels are sequential routing decisions, not alternate implementations of the same
classifier. A message proceeds only after the prior level misses or cannot produce a usable
result.

### Level 1 — Deterministic Classification and Routing

Level 1 performs no embedding generation and no model inference. It must evaluate mechanisms in
this order:

1. **Phrase captures** for structured patterns, such as a selection command followed by a GUID.
2. **Token-set rules** for resilient natural-language variants.
3. **Example substring rules** retained for backward compatibility.
4. **Follow-up rules** that use the immediately prior user message to collect a missing value.

A successful match dispatches directly to the corresponding typed application request through the
normal router and tool path.

#### Token-Set Rule Contract

Token rules improve recall without becoming fuzzy matching:

- Normalize messages to lowercase tokens, remove punctuation, normalize whitespace, and apply a
  small explicit alias map such as `actors` to `actor`, `named` to `name`, and `conversations` to
  `conversation`.
- `AllOf` contains domain-anchor tokens that must all be present.
- `AnyOfGroups` contains alternatives; at least one token from every group must be present.
- `NoneOf` contains blocker tokens that reject an otherwise matching rule.
- A rule may include narrow phrase captures to extract a value only after its token criteria
  match.
- When multiple rules match, select the unique rule with the greatest specificity, measured from
  its required tokens and required alternative groups.
- An equal-specificity tie is ambiguous and must be treated as a Level 1 miss.

Rules must be narrow enough to prevent false positives. Require stable domain anchors such as
`actor`, `chat`, `session`, or `message`; do not route based on generic verbs like `show` or
`find` alone.

Deterministic behavior means the same normalized input and catalog produce the same result. It does
not mean a rule is universally correct: catalog rules require regression tests for both intended
matches and nearby non-matches.

### Level 2 — Embedding and Vector-Search Classification

Level 2 runs only after a Level 1 miss and only when semantic classification is enabled. It
generates an embedding for the raw message, searches a pre-seeded intent embedding index, applies
the configured confidence threshold and result limit, then resolves a confident result to the same
intent catalog used by Level 1.

Principles:

- The intent catalog is the source of truth. Stored embeddings are a regenerable index, not a
  second intent catalog.
- Embed only canonical, parameter-free intent examples. Do not persist user values, captured
  values, or follow-up answers as training/index data.
- Restrict semantic matching to intents that can be safely dispatched without bespoke parameter
  extraction.
- Keep semantic classification feature-gated and disabled until labeled evaluations establish an
  acceptable precision and fallthrough rate.
- A generation, storage, or search failure is a Level 2 miss. It must be observable but must not
  fail the chat request.
- Score thresholds and result limits are deployment configuration, validated through offline
  evaluation rather than guessed in code.

Level 2 is semantic and probabilistic, but its dispatch is deterministic once a catalog intent has
been selected.

### Level 3 — Governed LLM Routing and Conversational Fallback

Level 3 executes only when no deterministic or semantic route succeeds. Governance enforcement is
a precondition to every model inference.

First, invoke the agent with forced tool selection, such as
`ChatOptions.ToolMode = ChatToolMode.RequireAny`. This gives the model a final opportunity to bind
arguments and invoke a relevant registered tool through the framework's typed function-calling
pipeline. Do not introduce a separate LLM-to-JSON intent classifier; that would duplicate the
framework's tool-selection and typed argument-binding capabilities.

If forced tool selection fails, produces no tool call, or returns no usable result, log the
outcome and make one open agent turn with normal automatic tool mode. The open turn is the final
fallback for general knowledge and prompts that truly do not apply to a tool. Do not retry the
forced-tool request indefinitely.

## Classification Observability

Routing must emit structured, privacy-safe evidence sufficient to improve the catalog and diagnose
fallbacks:

- correlation identifier and chat-session identifier;
- matched level and mechanism (`capture`, `token-set`, `example`, `follow-up`, `semantic`,
  `forced-tool`, or `open-agent`);
- selected intent and tool, when available;
- token-rule specificity, semantic score, threshold, and result rank where applicable;
- semantic-disabled, ambiguity, low-confidence, unavailable-dependency, and forced-tool failure
  reasons;
- latency and outcome for each attempted level.

Do not log raw prompts, captured values, tool arguments, or model responses unless an approved
retention policy explicitly permits it. Follow
[agent-runtime-observability.md](agent-runtime-observability.md) and
[ai-policy.md](ai-policy.md) for redaction, hashing, audit, and governance requirements.

## Architecture Boundary

The required execution path is:

```text
Chat message -> classifier -> intent router or agent runtime
             -> scoped tool -> application execution gateway
             -> mediator pipeline -> command/query handler -> domain or infrastructure
```

Tools use the shared scoped execution gateway. They must not access persistence, repositories,
SQL, or the mediator directly. Commands and queries must not contain chat types, token matching,
embedding logic, markdown formatting, or tool-routing policy.

`IntentMatch` remains an internal classification result that routing can consume but external
callers cannot fabricate. This preserves the structural invariant that deterministic routing only
occurs after classification.

## Reliability Maturity Model

The routing levels are complemented by this tool-calling maturity model:

| Maturity level | Mechanism |
|---|---|
| 1 | Clear, action-oriented tool names |
| 2 | Tool descriptions with scope, prerequisites, effects, and representative trigger wording |
| 3 | Governed agent instructions that state when tools must be used |
| 4 | This document's Levels 1 and 2 deterministic/semantic classification |
| 5 | This document's Level 3 forced-tool inference |

Start with naming, descriptions, and instructions. Add deterministic or semantic catalog coverage
for common, critical wording with demonstrated routing gaps. Use forced-tool inference only after
the lower-cost routing levels miss.

## Testing and Change Management

Changes to this system require:

- normalization tests, including aliases and punctuation;
- token-rule tests for required tokens, alternative groups, blockers, captures, specificity, and
  ambiguity;
- regression tests for existing phrase and follow-up behavior;
- semantic fallback tests with semantic classification enabled and disabled;
- end-to-end routing tests that verify a deterministic prompt reaches the expected tool/result
  shape and a true miss reaches Level 3;
- labeled evaluation and rollout metrics before broadening token rules or enabling Level 2.

Pilot new token rules on one or two high-value intents. Measure deterministic-match rate,
semantic fallthrough, Level 3 fallthrough, and false-positive reports before migrating additional
intents.

## Related Documents

- [architecture.md](architecture.md) — repository-wide architectural authority.
- [tool-design-and-implementation.md](tool-design-and-implementation.md) — tool execution
  boundary and implementation requirements.
- [agent-governance-principles.md](agent-governance-principles.md) — governance before every
  inference.
- [agent-runtime-observability.md](agent-runtime-observability.md) — evidence and sensitive-data
  handling requirements.
