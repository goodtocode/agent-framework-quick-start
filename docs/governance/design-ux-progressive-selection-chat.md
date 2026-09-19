# Progressive Selection Chat — Architecture & Design

## Status
This is the universal, project-agnostic design standard for the **AI agent chat experience**
pattern used to walk a user down a hierarchical resource tree (definition → instance →
sub-resource → leaf) purely through conversational turns. It applies to any Microsoft Agent
Framework (MAF) / Microsoft.Extensions.AI (MEAI) based chat surface, in any solution built from
this template. Product-specific detail (the concrete hierarchy levels, tool names, and phrasing
for a given solution) belongs in that solution's own product documentation — read the applicable
product-specific journey document alongside this one, not instead of it.

This document assumes familiarity with
`docs/governance/architecture-intent-classification-and-routing.md`. That document governs how a
free-form message resolves to a tool call; this document governs what a tool call **returns** and
how the chat surface turns that return value into the next step of a guided journey.

## Problem Statement

Many products in this ecosystem expose data as a **hierarchy**: a top-level catalog of
definitions, each with many runtime instances, each instance producing multiple parallel
sub-resources, each sub-resource containing many leaf records. A user rarely wants to type a GUID.
They want to be walked, level by level, from "what exists" down to "the specific leaf record I
care about," using natural language and lightweight taps/clicks, without losing conversational
context between levels.

This is the **Progressive Selection Chat** pattern: an agent chat surface that (1) always shows the
user what can be selected at the current level, (2) lets a single click/tap or short phrase advance
to the next level, (3) silently carries forward selected-context so the user never has to repeat an
identifier, and (4) exposes level-appropriate suggested prompts that change as context narrows.

## Core Principles

1. **The hierarchy is the journey, not the entity list.** Do not design chat tools around "CRUD
   for entity X." Design them around "what is the next question a user asks after selecting the
   previous level." A tool's response should always answer that next question or offer the means
   to ask it.
2. **Selection is a first-class response artifact, not prose.** When a tool response enumerates
   selectable children, it must emit a machine-parseable **selection token** for each one, in
   addition to human-readable markdown. The UI parses these tokens into clickable chips
   independent of exact wording, so selection works even when the model paraphrases the
   surrounding text.
3. **Context accumulates and narrows scope automatically.** Selecting an item at level N sets an
   "active context" server-side (e.g., active pipeline, active execution, active timeline) that
   subsequent tool calls read implicitly. The user is never asked to repeat an id they already
   selected. Explicit ids remain a supported override for direct/deep-linked requests.
4. **Suggested prompts are context-derived, not static.** The set of suggested next prompts shown
   beneath the composer must be computed from current selection state (nothing selected → top of
   hierarchy prompts; kit selected → instance-level prompts; instance selected → sub-resource
   prompts; sub-resource selected → leaf-level prompts). This keeps the surface useful without
   requiring the user to recall tool names.
5. **Every response ends with a "next suggested action."** A tool's markdown reply should
   explicitly state what a user would logically do next ("show chronicles or timelines for the
   selected execution"), reinforcing the guided-descent shape of the journey even for users typing
   free-form text instead of clicking chips.
6. **Parallel siblings are presented together, not hidden behind a single path.** Where a level has
   more than one class of child that a user would reasonably want to view side-by-side (e.g., a
   narrative summary artifact and a raw event-stream artifact produced from the same run), the
   response/journey should surface both as parallel, independently selectable branches rather than
   forcing a single linear order.
7. **The chat surface is single-turn stateless from the model's perspective; state lives in
   context, not conversation replay.** Long-running or asynchronous actions (e.g., "run this")
   must not promise future proactive notification — the assistant has no channel to push updates.
   Replies must say plainly that the user should send a new message to check status.
8. **Selection is idempotent and re-selectable.** Clicking a chip for an already-active selection,
   or starting a new chat session, must cleanly reset/replace context rather than accumulate stale
   state.

## Selection Token Contract

A tool response that offers selectable children emits one token per child using a stable,
delimiter-based grammar that is trivial to parse with a single regular expression and is resilient
to the model rephrasing surrounding prose:

```
[selection|<kind>|<id>|<value>|<label>]
```

- `kind` — the level/type of the thing being selected (e.g., `pipeline`, `execution`, `timeline`).
- `id` — the stable identifier used to re-query this item (GUID, code, canonical key).
- `value` — an optional secondary value (CURI, canonical key) carried for downstream tool calls.
- `label` — the human-readable chip label.

Rules for this contract:
- Tokens are additive to markdown, never a replacement for it — the markdown table/list remains
  the source of truth for what a human reads; tokens are only for UI chip extraction.
- The UI-side parser must be permissive of the model surrounding the token with arbitrary prose,
  and must degrade gracefully (no chips rendered, plain text still shown) if a token is malformed
  or absent.
- A generic per-`kind` prompt-builder (e.g., "Select pipeline `{id}`") must exist so new `kind`
  values can be introduced without UI code changes, with an explicit fallback for unrecognized
  kinds.

## Suggested Prompts Contract

Suggested prompts are a pure function of accumulated selection context, evaluated client-side or
server-side every time context changes:

- No selection → prompts that enumerate the top of the hierarchy and orient a first-time user.
- Level 1 selected (kit/definition) → prompts that reveal level 2 (instances) and the
  definition's own descriptive sub-views (its declared shape/plan/config).
- Level 2 selected (instance/run) → prompts that reveal both parallel level 3 branches together.
- Level 3 selected (a specific parallel branch) → prompts that reveal level 4 (leaf records) scoped
  to that branch.

This list must be short (3–4 items), action-oriented, and phrased as the literal sentence the tool
router expects, so a click reliably resolves to the same tool call a typed phrase would.

## Action Chip Contract

Selection chips and suggested-prompt buttons are visually distinct but behave identically: both
ultimately submit a phrase into the same message pipeline used for typed input. Do not create a
separate code path for "chip click" vs. "typed message" beyond constructing the phrase — this
guarantees intent routing, context updates, and reply rendering stay consistent regardless of
input method. Once a chip is acted on, it must be cleared from the strip so stale, already-resolved
selections are not offered again after the assistant's next reply.

## Anti-Patterns to Avoid

- Requiring the user to know or type a GUID/id to move between levels when a prior response already
  offered a chip for it.
- Tool responses that describe children in prose only, without emitting selection tokens.
- Suggested prompts that remain static regardless of context — this defeats the purpose of guided
  descent and reintroduces a "remember the tool name" burden on the user.
- Promising asynchronous follow-up ("I'll let you know when it's done") from a stateless,
  single-turn chat surface.
- Collapsing parallel sibling branches into a forced single linear path when the underlying data
  model treats them as independent, co-equal views of the same instance.

## Applicability

This pattern applies to any product built from this template that exposes a definition → instance
→ parallel-artifact → leaf-record hierarchy through an MAF/MEAI chat surface. Implementers should
reuse the selection-token grammar, the context-accumulation model, and the suggested-prompt-by-level
contract rather than inventing a bespoke variant per product.
