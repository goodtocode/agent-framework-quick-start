# Ontology

## Purpose
Authoritative source for domain terminology and ubiquitous language.

## Usage Rules
- Define what exists in the domain, not behavior over time.
- Use canonical terms in code, docs, APIs, and tests.
- Resolve synonyms here before feature implementation.

## Core Concepts
- **Tenant**: organizational boundary for protected data and capabilities.
- **User**: authenticated person interacting with application capabilities.
- **Actor**: the persisted representation of a user used by application records.
- **Chat Session**: an owned conversation context containing ordered messages.
- **Chat Message**: a user, assistant, or system message in a chat session.
- **Agent**: an AI-driven component that receives governed chat context and may invoke tools.
- **Tool**: a typed, scoped capability exposed to the agent through the application request pipeline.
- **Governed Inference**: an agent invocation with explicit observability, auditability, defensibility, and repeatability metadata.

## Relationships
- A Tenant contains Users, Actors, Chat Sessions, and Chat Messages.
- A User maps to an Actor and owns Chat Sessions within a Tenant.
- A Chat Session contains ordered Chat Messages.
- An Agent responds to Chat Messages and can invoke scoped Tools.
- A Governed Inference records the context for an Agent response.

## Synonyms
- Conversation: chat session (prefer **Chat Session** for persisted context).
- Plugin/function: tool (prefer **Tool** for an agent capability).

## Invariants
- Each concept has one canonical term.
- Terms in feature documents must map to concepts here.

## Out of Scope / Deferred
- Durable workflow orchestration, long-running runs, and domain-specific evaluation are optional extensions.

## Definitions
When new terms are introduced, add definitions here before feature implementation.
