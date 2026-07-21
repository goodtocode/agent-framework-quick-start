# Sprint 0 Process

## Purpose
Define the repeatable Sprint 0 process for creating business context before implementation.

## Timebox and Intent
- Sprint 0 is a design-and-enablement sprint, not a feature delivery sprint.
- Sprint 0 is strictly time-boxed to two weeks.
- The sequence is intentional: meaning first, then behavior, then delivery setup.

## Preconditions
- Epic scope is stated.
- Participants agree to canonical language ownership.

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
