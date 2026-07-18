# Ontology Definition
**Sprint 0 Foundational Design Step**

---

## Purpose

Ontology Definition establishes a **shared, stable domain language** for the epic.

Its purpose is to:
- Define what exists in the domain
- Remove ambiguity from terminology
- Prevent conceptual drift across teams, time, and implementations
- Create a durable semantic foundation for behavioral modeling and delivery

Ontology is concerned with **meaning**, not behavior or implementation.

---

## When This Occurs

Ontology Definition is the **first design activity in Sprint 0**.

It must be completed *before*:
- Event Storming
- Architectural decisions
- Backlog creation

Event Storming assumes the ontology exists.

---

## Core Questions Answered

Ontology answers:
- What are the core concepts in this domain?
- What does each concept *mean*?
- Which concepts are distinct vs. synonymous?
- How are concepts related?
- What invariants must always hold true?

Ontology does **not** answer:
- What happens over time
- What triggers behavior
- How the system is implemented

---

## Scope and Constraints

Ontology should include:
- Core entities and aggregates
- Supporting domain concepts
- Explicit terminology definitions
- Relationships between concepts

Ontology should **exclude**:
- UI concepts
- Infrastructure concepts
- Implementation details
- Workflow sequencing

The ontology must converge within Sprint 0.

---

## Output Artifact

The outcome of this step is a **documented domain ontology**, typically including:
- A list of canonical terms
- Definitions in plain language
- Concept relationships
- Notes on assumptions and exclusions

The ontology is a **living artifact**, but changes after Sprint 0 must be intentional and visible.

---

## Design Principles

- Prefer nouns over verbs
- Prefer clarity over completeness
- Prefer shared language over local optimization
- Make ambiguity explicit rather than implicit

---

## Success Criteria

Ontology Definition is successful when:
- Stakeholders use the same words to mean the same things
- Event storming can proceed without renaming core concepts
- The epic scope can be described using only ontological terms