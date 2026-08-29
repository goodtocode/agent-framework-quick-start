# UX Foundation

## Purpose

Define the baseline interaction model for the Quick Start's governed chat experience and the principles that future operational surfaces must preserve.

This document precedes visual styling, page implementation, and component-specific design decisions.

---

## Product Identity

The Quick Start is a practical foundation for a governed, tool-enabled business chat application. It is not a marketing site or a complete operations platform; it demonstrates durable interaction and architecture defaults that products can extend.

---

## Core UX Principles

### Conversation Is the Record

The ordered user and assistant bubbles are the durable conversation record. UI controls can initiate or continue a conversation, but they do not masquerade as messages.

### Context Before Action

Tool actions should occur only after the user has enough context to understand the effect. Consequential writes require explicit confirmation.

### Progressive Disclosure

Show the active session and transcript first. Suggested prompts and follow-up actions reveal the next useful action without displacing conversation context.

---

## Chat
- Preserve ordered user and assistant bubbles as the durable conversation record.
- Auto-send suggested prompts through the same path as typed input.
- Render tool follow-up actions outside the transcript and remove their transport metadata from assistant content.
- Disable input while a message is submitting and make failure states actionable.

---

## Operations
When an admin surface is added, follow [style-guide-admin.md](../../governance/style-guide-admin.md): records first, selected context second, actions after context.