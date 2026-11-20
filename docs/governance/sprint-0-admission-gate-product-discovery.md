# Sprint 0 Discovery and Readiness Guidance

## Purpose

Use this guidance to assemble enough business context to make useful progress on an epic. Adapt the amount of evidence, review, and formality to the team's size, resources, uncertainty, and risk. A solo founder can make and record decisions without inventing separate roles, committees, or a formal approval ceremony.

The epic document can be the discovery packet. It can start with an event list and experience flow and grow as questions are answered. A complete packet, a checklist, a named reviewer for every role, and a recorded GO/NO-GO decision are not universal prerequisites for beginning discovery or drafting Sprint 0 assets. Teams with contractual, regulatory, organizational, or higher-risk approval requirements should follow those requirements and may use a formal gate.

## Work Iteratively

1. Capture the epic's intended outcome, people, scope, known rules, representative journey, and important exceptions using the evidence available.
2. Record assumptions, unanswered questions, and dependencies rather than filling gaps with invented decisions. Link supporting evidence when available.
3. Add relevant candidate concepts, relationships, flows, and system boundaries to the shared [Sprint 0 assets](../product/sprint-0/), clearly marked as provisional and linked to their source epic. Refine them as subsequent epics arrive; do not wait for a big-bang documentation pass.
4. Reconcile cross-epic terminology and boundaries when enough evidence exists. Use the ontology before formal event storming, and derive implementation-sized features from the resulting understanding.
5. Before committing to implementation, review the affected scope for sufficient product clarity, feasibility, access controls, security, operational readiness, and unresolved decisions. Resolve blockers or explicitly exclude the dependent scope.

These are activities, not a required document sequence. New evidence can change earlier drafts. A draft is not proof of implementation, approval, or an external commitment.

## Scale the Review

| Situation | Suggested approach |
|---|---|
| Solo founder or small team | Use a concise epic, a flow or sketch when useful, and a short list of decisions and open questions. The same person may own product, design, and technical review. |
| More stakeholders or significant uncertainty | Add relevant reviewers, explicit responsibilities, supporting evidence, and visual or state examples where they help expose risk. |
| High-impact, regulated, contractual, or externally owned work | Add appropriate approvals, traceable evidence, formal decision records, and external-owner confirmation as required by the circumstances. |

Use judgment about what needs review before discovery, what can be investigated during it, and what must be settled before implementation or release. Security, privacy, access, and external authority questions must not be treated as resolved just because a document exists.

## Useful Evidence

Capture only what helps the work, expanding it as appropriate:

- Opportunity, intended outcome, customer value, and possible success evidence.
- Primary actors, stakeholders, responsibilities, included and excluded scope.
- A representative journey with meaningful exception and recovery paths.
- Experience flow, sketches, screenshots, or state notes where they clarify an uncertain interaction.
- Candidate business events, consequential rules, constraints, and external dependencies.
- Evidence versus assumptions, existing capability and feasibility, and product/platform responsibilities.
- Resolved decisions with rationale; open questions with an owner when known and a next step.

Keep unresolved cross-cutting intent in the existing [intent gap register](../product/features/intent-gap-register.md), and link decisions and closure evidence from the owning epic. Do not treat unknown current capability as implemented.

## Suggested Epic Outline

Use or adapt these headings; omit or combine sections that do not help:

```markdown
# Epic Definition: <name>

## Opportunity and Intended Outcome
## People and Scope
## Representative Journey and Experience Flow
## Candidate Business Events
## Business Rules and Constraints
## System Context
## Evidence and Assumptions
## Open Questions
## Success Evidence
## Related Sprint 0 Assets
```

Store epics as `epic-<identifier>-<name>.md` in [product feature documentation](../product/features/); they are epic-level documents, not feature definitions. Keep shared terminology in the [ontology](../product/sprint-0/ontology.md), shared journeys in [user flows](../product/sprint-0/user-flows.md), and concise cross-epic context in the [context overview](../product/sprint-0/context-diagram.md), linking to epic-specific diagrams rather than duplicating them.

## From Discovery to Delivery

The [Sprint 0 process](sprint-0-process.md) and [methodology](sprint-0-basic-overview.md) provide the timebox, design sequence, and delivery goals. Adapt the discovery practices proportionately, and preserve the distinction between provisional discovery, agreed product decisions, and implementation readiness. Creating an epic or a draft model does not itself authorize implementation.
