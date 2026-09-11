# Intent Classification and Tool Routing — Architecture & Design

## Status
This is the universal, project-agnostic design standard for intent classification and AI tool
routing in any Microsoft Agent Framework (MAF) / Microsoft.Extensions.AI (MEAI) based chat
surface in this workspace. It applies to `crucible-web` and `agent-framework-quick-start` alike.
Repository-specific detail (tool catalogs, example phrases, config values, data model,
known gaps) lives in `docs/product/features/feature-Intent-classification-and-routing.md` -
read that document alongside this one, not instead of it.

## Primary Use Case vs. Secondary Use Case

**This is the single most important framing in this document.** Every design decision below
exists to serve one primary use case; anything else is explicitly out of scope until that use
case is solid.

- **Primary use case (this document's entire scope): classify a user's free-form message and
  route it to the correct tool call, reliably.** A chat surface backed by MAF/MEAI tools exists
  first and foremost to let a user's natural-language request resolve to the right registered
  tool being invoked with the right arguments, every time. Everything in this document - the
  tiered cascade, the classifiers, the architecture boundary - exists to make that resolution
  step deterministic, fast, and reliable.
- **Secondary use case (explicitly future/deferred): using the data a tool call returns to
  provide analysis, guidance, or remediation recommendations.** Once a tool has been reliably
  invoked and has returned data, a later concern is *what the assistant does with that data*
  (e.g., "here's what's wrong and here's how to fix it"). That concern is **not** addressed by
  this document and must not influence intent-classification/routing design decisions. Do not
  conflate "did we call the right tool" with "did we give good advice about the tool's result" -
  they are different problems solved at different times.

## Design Priority Order (What To Get Right First)

In strict order of design priority for the primary use case:

1. **Deterministic intent classification** - matching a message to an intent with no model or
   embedding call. Parameter extraction/handling for parameterized intents is explicitly **not**
   a blocking design concern at this stage; if a parameterized intent needs more design later,
   defer it rather than blocking classification design on it.
2. **Embedding (semantic) classification** - a second, still-deterministic-dispatch pass using
   vector similarity for phrasings not covered by exact rules. Same deferral applies: parameter
   handling for parameterized intents can be designed later; this tier's initial scope is
   non-parameterized intents only.
3. **LLM classification, routing, and parameter handling as part of MAF** - only once tiers 1 and
   2 miss does the model itself get involved, and only at this tier does full argument/parameter
   extraction become MAF's responsibility (its own typed tool-calling/argument binding), not a
   bespoke parsing layer.

This ordering exists because it is a cost/latency/reliability pyramid: free and 100% reliable,
then cheap and probabilistic, then expensive and non-deterministic - always try the cheaper,
more reliable mechanism first.

## The Tiered Cascade

Four mechanisms, grouped into two tiers by one property: **does it ever call the AI agent/MAF?**

```text
Tier 1 - Deterministic Intent Classification (NEVER calls the AI agent / MAF)
    Tier 1a: Rule classifier              substring + capture match, zero cost
    Tier 1b: Semantic (embedding) classifier   vector search, zero MAF cost
    Both resolve straight to the same deterministic dispatch/route - from the router's
    perspective, a Tier 1a hit and a Tier 1b hit are indistinguishable.
    |
    v (only when BOTH 1a and 1b miss)
Tier 2 - MAF (the AI agent is actually invoked; raw prompt -> model decides the tool)
    Tier 2a: Forced-tool inference         ToolMode = RequireAny (model must call some tool)
    Tier 2b: Open agent turn               ToolMode = Auto (default, final fallback)
```

```text
User message
    |
    v
Tier 1a: Rule classifier                 -- no LLM, no embedding call
    |
    +--> Match --------------------------------------> Deterministic route
    |
    +--> Miss, semantic disabled ----------------------> Tier 2
    |
    +--> Miss, semantic enabled
            |
            v
        Tier 1b: Semantic classifier        -- 1 embedding call + vector search
            |
            +--> Confident match on a NON-parameterized intent --> Deterministic route
            |
            +--> No match / low confidence / parameterized intent / any exception --> Tier 2
            |
            v
Tier 2a: Forced-tool inference (ToolMode.RequireAny) -- 1 chat completion, tool call forced
    |
    +--> Model calls a registered tool ---------------> Tool result is the reply
    |
    +--> Exception / empty / no tool call -------------> Tier 2b (no retry)
            |
            v
Tier 2b: Open agent turn (ToolMode.Auto, default) -- general knowledge / anything not tool-shaped
```

**Why an embedding classifier is not "a second MAF tier"**: the semantic classifier never touches
the AI agent. It does its own vector search against a pre-seeded embedding store and, on a
confident hit, dispatches straight to the same deterministic route a rule match would use. It is
a second *technique* for reaching Tier 1's outcome (skip the model entirely), not a variant of
Tier 2. The only place "send the raw prompt to the model and let it decide" happens is Tier 2.

### Tier 1a - Deterministic Rule Classifier
Matches the raw message against known-good phrasings and parameterized captures, in order:
1. Parameterized phrase captures (e.g. a fixed prefix followed by an ID/name), checked first so
   parameterized intents win over broad phrase matches.
2. Exact/substring example phrases.
3. Follow-up examples matched against the *prior* message, for collect-missing-parameter
   follow-up turns.

100% reliable for the phrasings/capture shapes it covers. No model call, no embedding call - free
and instant. Maps to Level 4 in the "5 Levels of Tool-Calling Maturity" model below.

### Tier 1b - Semantic (Embedding) Classifier
On a Tier 1a miss, if semantic classification is enabled, generate a query embedding for the raw
message, cosine-similarity search a pre-seeded embedding store, and resolve the best match back to
an intent - **scoped to non-parameterized intents only** at this stage (per the Design Priority
Order above; parameterized-intent semantic matching is deferred future work, not a current
requirement).

Design principles:
- **The canonical intent catalog (its stable name, example phrases, and capture definitions) is
  the single source of truth.** Stored embeddings are a regenerated index over that source, like
  a database index - never a second source of truth. Changing the embedding model does not
  require code changes, only regenerating the cache.
- **Only canonical, parameter-free example phrases may ever be embedded.** User-supplied values,
  missing values, and values collected via follow-up prompts must never be embedded - those stay
  on deterministic captures (Tier 1a) or MAF's own typed tool-calling (Tier 2).
- **Disabled by default, behind a feature flag**, so semantic classification can be deployed
  dormant and enabled only after accuracy is measured, with instant rollback to deterministic-only
  behavior.
- **Any embedding-infrastructure failure (generation error, store unavailable) must be treated as
  "no match" and fall through, never break the chat request.**
- **Weighted scoring by source** is a reasonable refinement once basic matching works: example
  phrases (highest trust, user-facing wording), tool/method descriptions (lower trust, author
  wording), tool metadata (lowest trust, derived fields) can each carry a different weight in
  similarity scoring rather than being treated as equally authoritative.
- **Confidence threshold and top-K result count must be configurable**, not hard-coded, so they
  can be tuned via offline evaluation against labeled examples without a code change.

### Tier 2a - Forced-Tool Inference
If Tier 1 misses entirely, the **same** agent (same tool catalog, same instructions) is invoked a
second time for this one turn with tool selection forced (e.g. MEAI's
`ChatOptions.ToolMode = ChatToolMode.RequireAny`), so the model must call **some** registered tool
rather than replying with only an announcement of intent ("I will look that up...") or a
conversational refusal. If the model successfully invokes a tool, the framework's own
function-invocation pipeline executes it and binds arguments - **this is where parameter/argument
extraction for parameterized intents belongs**, using the framework's typed argument binding, not
a bespoke JSON/regex parsing layer. Any exception, empty response, or non-conformant behavior is
logged and treated as "no match" - falls straight through to Tier 2b with **no retry**, and must
never surface to the caller.

### Tier 2b - Open Agent Turn
The default, unforced behavior: the agent runs with its normal tool mode (e.g. `Auto`). The model
may call a tool or answer conversationally. This is where general knowledge and anything not
tool-shaped is answered - it is explicitly a last resort, not the primary interaction mode for a
tool-routing chat surface.

## The 5 Levels of Tool-Calling Maturity (Universal Reliability Model)

A separate, complementary maturity model for making tool-calling reliable, to be applied **in
order** - do not skip to a later level "because it seems more robust." Most tools only need
Levels 1-3; only add Level 4 for phrasings that repeatedly fail 1-3; only consider Level 5 if
Level 4's hard-coded phrasing list becomes unmanageable for a given tool.

| Level | Name | Mechanism |
|---|---|---|
| 1 | Naming | Tool/method names must be unambiguous, action-oriented, and not aliasable to the model's own "I can help with that" conversational instincts. |
| 2 | Descriptions | Every tool method carries a description that states what it does, lists concrete (including vague/indirect) trigger phrases, and gives an explicit imperative directive to always call it and never guess, refuse, or ask permission. |
| 3 | Agent instructions | Explicit, per-tool routing instructions injected into the agent's system prompt, authored as strongly typed configuration (not a single hard-coded string), reinforcing when to call each tool and forbidding observed failure phrasings. |
| 4 | Deterministic routing | This document's Tier 1a: known-good phrasings bypass model tool-selection entirely, matched server-side, guaranteeing 100% reliability for those phrasings. |
| 5 | Forced-tool inference | This document's Tier 2a: reuse the framework's own "force a tool decision now" primitive (e.g. `ToolMode.RequireAny`) rather than inventing a bespoke intent-classification prompt/JSON contract. |

**Diagnostic checklist when a tool isn't being called reliably**: (1) does the description list
this phrasing as a trigger example - if not, that's a Level 2 gap; (2) do the agent instructions
explicitly forbid the observed failure phrasing - if not, Level 3 gap; (3) is this phrasing
important/common enough to guarantee regardless of model behavior - if yes, add Level 4; (4)
confirm the agent call is a single non-streaming turn before assuming a deeper bug - an
"announcing intent" reply on a single non-streaming call *is* the complete final-turn output, not
evidence of a paused/dropped callback, it is simply the wrong output for that turn.

**Change management**: reproduce with a live-AI end-to-end measurement first, apply Level 2, retest,
apply/strengthen Level 3, retest, apply Level 4 if still insufficient, retest. Only propose a
Level-5-style mechanism as a design discussion, not a unilateral implementation, and only once
Level 4 becomes unmanageable for a given phrasing set.

## Why Not A Bespoke LLM-Driven JSON Intent Classifier (Tier 2a Design Rationale)
An earlier design considered composing multiple classifier stages behind one interface, including
a hypothetical future "semantic classifier" that would ask the model to return a JSON
`{ intent, captures }` payload matched against a private intent catalog. That design was rejected:

1. **It duplicates the framework's own function-calling instead of using it.** A "force a tool
   decision now" primitive already exists in modern agent frameworks, with better argument binding
   (the framework's own typed parameter binding, not hand-rolled JSON/regex parsing) and zero risk
   of drift between "the classifier's private intent catalog" and "the tools that actually exist."
2. **It adds an interface with no behavioral difference from what it wraps** - composing a single
   classifier behind an abstraction is not a meaningful abstraction on its own.
3. **A genuinely new abstraction requires an actual second implementation with different
   behavior.** Forced-tool inference *is* that second, meaningfully different mechanism - it does
   not need to hide behind the same interface as the deterministic matcher, because it isn't a
   classifier in the same sense: it's a full agent turn that both decides *and* executes via the
   framework's own tool-invocation pipeline.

**Rule going forward**: only add an internal abstraction (interface, pipeline, extra class) when
it has more than one real implementation with genuinely different behavior, or when it is required
to enforce a structural invariant (see below). Do not add abstractions to "leave room" for a
future tier unless that tier is being implemented now. This applies equally to the embedding
classifier: it matches against pre-seeded example embeddings, not a model-generated JSON payload,
and is a narrower, different mechanism from what this section argues against - the rejection above
is scoped to Tier 2a's design, not a statement against Tier 1b.

## Structural Invariant: Classify Before Route
A classified intent match must be a type that can only be constructed by a classifier
implementation (e.g. an internal constructor scoped to the classification namespace), so that the
routing/dispatch method is structurally unreachable without classification having happened first.
This is a compiler-enforced guarantee, not just a naming convention or code-review expectation.

## Architecture Boundary: Keep Business Logic Pure
Application-layer commands/queries (pure business logic/DDD) must never be aware of:
- **Presentation concerns.** A chat surface is one presentation surface among many (web UI, API
  client, future channels). Business logic must not format markdown tables, build UI action
  affordances, or otherwise shape output for a specific rendering surface.
- **Regex-based intent matching.** Deciding "which capability does this free-form message map to"
  is a routing/presentation concern, not a business rule.
- **Agent-framework/model types.** Chat-message, agent, or tool-calling types must never appear in
  a business command/query request, handler, or validator.

Business commands/queries take a minimal, strongly typed request; perform their operation; and
return a strongly typed result - nothing else. Everything else (tools, the routing service that
owns deterministic matching + markdown/text formatting + the model fallback, and agent-instruction
composition) belongs in the infrastructure/presentation-adjacent layer that talks to the agent
framework, behind a narrow abstraction the business layer depends on. Enforce this with an
automated architecture guard (a test that scans business-layer source for forbidden references)
rather than relying on code review alone.

## Reliability Evidence Behind This Design
This design is not theoretical - it is backed by measured, reproducible results:
- **Raw/unforced model tool-selection failed on the order of 75% of prompts** in real testing,
  with a specific "announce intent, no follow-up possible" failure mode (e.g. "I will fetch that
  for you" with no way to complete the action in the same turn, since a single non-streaming agent
  call's output *is* the complete final-turn output).
- Strengthening descriptions and agent instructions alone measurably improved but did not fully
  close this gap for some tools/phrasings - some tools plateaued around a real-world ceiling well
  short of 100% reliability using model-based routing alone.
- Deterministic routing (Tier 1a) closed the gap completely for the phrasings it covers, with zero
  marginal cost per request.
- This is why Tier 1 (deterministic, then semantic) is treated as the primary, load-bearing
  mechanism for tool routing, and Tier 2 (the model) is treated as a fallback for cases Tier 1
  cannot yet cover - not the other way around.

## Related Documents
- `docs/product/features/feature-Intent-classification-and-routing.md` - repository-specific tool
  catalog, example phrases, configuration values, data model, and known implementation gaps.
