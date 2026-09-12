# Progressive Selection UI — Architecture & Design

## Status
This is the universal, project-agnostic design standard for the **web/mobile page-level UI**
scaffold that hosts a Progressive Selection Chat surface (see
`docs/governance/design-ux-progressive-selection-chat.md`, which this document assumes and does
not repeat). This document covers layout, component boundaries, and responsive behavior — not the
chat protocol itself. Product-specific detail belongs in that solution's own product
documentation.

## Problem Statement

A single chat page must serve two audiences at once: someone picking up a prior conversation
(session history) and someone actively working a guided hierarchy descent (message list +
selection chips + suggested prompts + composer). The layout must work identically in narrow
(mobile) and wide (desktop) viewports without diverging behavior, and must never require a page
navigation to move between hierarchy levels — the entire journey happens inside one page.

## Core Principles

1. **One page hosts the entire journey.** Selecting a kit, an instance, a parallel artifact branch,
   and a leaf record all happen without route changes. The page is a shell around: session
   picker, message history, action/selection chips, suggested prompts, and composer. Routing to a
   dedicated per-entity page is reserved for the CRUD/admin experience, not the chat journey.
2. **Breakpoint-driven layout, not device-detection.** Layout responds to a measured grid
   breakpoint (e.g., narrow vs. standard), not user-agent sniffing. Both layouts render the same
   five regions in the same top-to-bottom order; only proportions/placement (stacked strip vs.
   side list) differ.
3. **Five stable regions, always present in this order:**
   - **Session control** — start a new session; access session history.
   - **Header / orientation** — a constant reminder of the page's purpose ("What can I help you
     with?").
   - **Message history** — scrollable, bounded height, auto-scrolls to newest message on new
     content.
   - **Action strip** — the current selection chips (see selection-token contract in the chat
     governance doc), directly above the composer.
   - **Composer + suggested prompts** — the input control immediately followed by the
     context-derived suggested-prompt buttons, so the two "ways to advance the journey" (type vs.
     tap a suggestion) are visually adjacent.
4. **Session switching resets journey context.** Choosing a different session or starting a new
   one must clear all accumulated selection state (active kit/instance/branch/leaf) and pending
   action chips — a session boundary is also a context boundary.
5. **The message list re-derives chips on every refresh, not just once.** After any message is
   sent or a session is (re)loaded, the UI re-scans the latest assistant reply for selection
   tokens and rebuilds the action strip and suggested prompts from that scan — chips are a
   projection of the latest reply, never independently persisted UI state.
6. **Scroll-to-latest is an explicit, page-owned behavior.** The page, not the message list
   component, owns the "should scroll to bottom" flag and triggers it after render whenever new
   content arrives (new message, new session, session switch).
7. **Chips and suggestions are ephemeral per turn.** Selection chips are cleared immediately when
   acted upon (before the resulting message round-trip resolves) to prevent double-submission and
   to avoid presenting stale, already-resolved options while a new reply is in flight.
8. **No client-side business logic duplication.** The page composes existing tool/service replies;
   it must not re-derive domain state (e.g., recompute execution status) that a tool call already
   returned. UI logic is limited to: session bookkeeping, breakpoint layout, token parsing for
   chips, and suggested-prompt selection based on locally-tracked context ids.

## Component Boundaries

- **Page (shell)**: owns session state, active-context ids (kit/instance/branch/leaf), pending
  action chips, breakpoint flag, scroll-flag, and orchestrates calls to the chat service.
- **Session list/strip**: pure presentation of available sessions; emits a selection event only.
- **Message list**: pure presentation of the conversation; owns only the regex-based extraction of
  selection tokens for its own rendering needs (e.g., stripping tokens from displayed text), not
  for building the action strip (that responsibility stays with the page so it can coordinate with
  submitted-context state).
- **Action strip**: stateless renderer of the chips the page hands it; emits a click event with the
  full selection payload (kind/id/value/label) and takes no action itself.
- **Suggested-prompts strip**: stateless renderer of prompt strings computed by the page; emits the
  clicked string verbatim as the next submitted message.
- **Composer (input or card variant)**: the only component that talks directly to the chat
  service to create a session or submit a message; both mobile and desktop layouts wrap the same
  submission API so the page can drive either variant without branching its own logic beyond
  choosing which ref to call.

## Responsive Layout Rules

- Narrow layout: session control and session strip stack full-width above the header; message
  history, action strip, and composer/suggestions stack full-width below.
- Standard/wide layout: session control and session list occupy a persistent side column; message
  history occupies a wide center column; action strip and composer/suggestions occupy a matching
  wide column beneath the message history, vertically aligned with it.
- Both layouts must present the same maximum message-history height with independent scrolling
  (never let the whole page scroll to reveal older messages — only the history region scrolls).

## Anti-Patterns to Avoid

- Navigating to a different route to represent progression through the hierarchy — this breaks the
  "single continuous conversation" mental model the chat pattern depends on.
- Persisting selection chips independently of the latest assistant reply (leads to stale/duplicate
  chips after a session switch or new reply).
- Device-specific business logic branches beyond the composer variant used.
- Letting the message-history component own submission or context-tracking responsibilities that
  belong to the page shell.

## Applicability

This layout scaffold applies to any product page, in any solution built from this template,
hosting a chat-driven guided journey. Reuse this five-region, breakpoint-driven structure rather
than inventing a new page layout per product.
