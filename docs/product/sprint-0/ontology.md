# Ontology

## Purpose
Authoritative source for domain terminology and ubiquitous language.

## Usage Rules
- Define what exists in the domain, not behavior over time.
- Use canonical terms in code, docs, APIs, and tests.
- Resolve synonyms here before feature implementation.

## Core Concepts
- **Tenant**: organizational boundary for users and assets.
- **User**: person interacting with application capabilities.
- **Asset**: monitored digital resource requiring classification and actions.
- **Agent**: AI-driven component that orchestrates analysis or mitigation.
- **Work Item**: actionable unit generated from detections or user intent.

## Relationships
- A Tenant contains many Users and Assets.
- Assets can generate multiple Work Items.
- Agents analyze Assets and propose actions for Work Items.

## Synonyms
- Work Item: task, ticket (avoid in code unless required by integration).
- Asset: resource, monitored entity (prefer **Asset**).

## Invariants
- Each concept has one canonical term.
- Terms in feature documents must map to concepts here.

## Out of Scope / Deferred
- Detailed event timelines and workflow ordering (captured in event storming/user flows).

## Definitions
When new terms are introduced, add definitions here before feature implementation.
