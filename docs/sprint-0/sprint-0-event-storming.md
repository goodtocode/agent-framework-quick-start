# Event Storming
**Sprint 0 Behavioral Modeling Step**

---

## Purpose

Event Storming models **how the system changes over time** using domain events grounded in a stable ontology.

Its purpose is to:
- Reveal system behavior
- Discover rules, decisions, and responsibilities
- Identify boundaries, integrations, and risks
- Validate that the ontology supports real workflows

Event Storming is concerned with **behavior**, not structure or implementation.

---

## Pre‑Conditions

Event Storming requires:
- An agreed‑upon domain ontology
- Canonical domain terminology
- Clear epic scope boundaries

Event Storming must not redefine core nouns.

---

## Core Questions Answered

Event Storming answers:
- What happens in the system?
- What triggers change?
- Who or what initiates actions?
- What rules or policies govern behavior?
- Where are the seams and boundaries?

Event Storming does **not** answer:
- How data is stored
- How services are implemented
- Detailed UI flows

---

## Scope and Constraints

Event Storming should include:
- Domain events (past tense)
- Commands (intent to change state)
- Actors (human or system)
- Policies / rules
- External integrations

Event Storming should **exclude**:
- Technical implementation details
- Low‑level infrastructure behavior
- Premature optimizations

The goal is clarity, not exhaustiveness.

---

## Relationship to Ontology

- All events must reference ontological concepts
- If an event cannot be named clearly, ontology must be revisited
- Ontology is input; events are validation

Event Storming may reveal **gaps or flaws** in the ontology, which should be resolved explicitly.

---

## Output Artifact

The outcome of this step is an **event model**, capturing:
- Key domain events
- Command → event relationships
- Decision points and policies
- Identified boundaries and responsibilities

This artifact feeds directly into:
- Bounded context definition
- Backlog shaping
- Architectural decisions

---

## Design Principles

- Events are facts, not actions
- Language must match the ontology
- Prefer simplicity over false precision
- Model reality, not desired architecture

---

## Success Criteria

Event Storming is successful when:
- The core flows of the epic are clearly understood
- Risks and unknowns are surfaced early
- Boundaries are visible
- The backlog can be shaped without major conceptual gaps