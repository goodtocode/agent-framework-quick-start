# Sprint 0 Process

## Purpose
Define the repeatable Sprint 0 process for creating business context before implementation.

## Timebox and Intent
- Sprint 0 is a design-and-enablement sprint, not a feature delivery sprint.
- Sprint 0 is strictly time-boxed to two weeks.
- The sequence is intentional: meaning first, then behavior, then delivery setup.

## Starting Context
- Use the [discovery and readiness guidance](sprint-0-admission-gate-product-discovery.md) in proportion to the team, resources, risk, and uncertainty.
- Start with the epic's available intent, an initial scope, and known questions; build the evidence and Sprint 0 assets iteratively.
- Identify who can make product and terminology decisions. In a one-person organization, these responsibilities can belong to the same person.
- Mark drafts and unresolved decisions clearly; use any formal approvals required by the work's circumstances.

## Step 1: Ontology Discovery (What Exists)
- Identify and clarify core concepts, definitions, relationships, synonyms, and invariants.
- Select canonical terms and record out-of-scope concepts.
- Output: ontology artifact approved for event storming.

## Step 2: Event Storming (What Happens)
- Discover domain events (past tense), commands, actors, and policies.
- Normalize event names against ontology.
- Identify boundaries, responsibilities, and candidate bounded contexts.
- Output: event storming artifact and boundary insights.

## Step 3: User Flows
- Refine and validate the journeys, flows, use cases, and visual evidence available as discovery proceeds.
- Capture success, alternate, and error paths.
- Output: `docs/product/sprint-0/user-flows.md`.

## Step 4: Feature Identification
- Convert ontology/events/flows into feature candidates.
- Output: prioritized feature list.

## Step 5: Backlog Creation
- Author feature docs using the repository feature template.
- Output: implementable feature backlog.

## Deliverables
- Product overview.
- Ontology.
- User flows.
- Prioritized feature list and initial feature documents.
- Delivery scaffolding readiness (repo, pipeline, IaC, identity baseline).
