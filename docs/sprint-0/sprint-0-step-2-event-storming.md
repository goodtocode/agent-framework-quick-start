# Event Storming — Conversation Guide
**Sprint 0 Behavioral Design Artifact**

---

## Purpose

This document guides collaborative **event storming** for a Sprint 0 epic.

Its goals are to:
- Model system behavior over time
- Surface rules, decisions, and responsibilities
- Validate that the ontology supports real workflows
- Reveal boundaries, risks, and complexity early

Event Storming defines **what happens**, not how it is implemented.

---

## Pre‑Conditions

Event Storming assumes:
- A documented, agreed‑upon ontology
- Canonical domain language
- Clear epic scope boundaries

If core nouns are still debated, return to ontology definition.

---

## Section 1: Conversation Guide

### Step 1 — Frame the Behavioral Scope

**Objective:** Align on what behaviors are being explored.

Define:
- Which scenarios are in scope
- Which scenarios are explicitly out of scope
- The level of detail expected

> The goal is clarity, not completeness.

---

### Step 2 — Event Discovery (Facts)

**Objective:** Identify domain events.

Ask:
- “What has happened in this domain?”
- “What facts do we care about after the fact?”

Rules:
- Events are named in **past tense**
- Events reference ontological concepts
- No commands, no UI actions, no technologies

Capture all candidate events.

---

### Step 3 — Event Normalization

**Objective:** Clean and align events.

Activities:
- Remove duplicates
- Split overloaded events
- Rename events to match ontology

If an event cannot be named clearly, note the issue explicitly.

---

### Step 4 — Causation Discovery (Commands & Actors)

**Objective:** Identify what causes events.

For each event:
- What command caused it?
- Who or what issued the command?

Rules:
- Commands express intent (present tense)
- Actors may be people or systems
- Do not design APIs or UIs

---

### Step 5 — Policy & Decision Discovery

**Objective:** Surface rules and logic.

Identify:
- Business rules
- Validations
- Approval steps
- Automated decisions

Policies explain **why** an event occurred, not **how** it was coded.

---

### Step 6 — Boundaries & Responsibilities

**Objective:** Identify seams in the system.

Look for:
- Ownership boundaries
- Responsibility handoffs
- External integrations
- Natural grouping of events and policies

These often suggest bounded contexts.

---

### Step 7 — Agreement & Closure

**Objective:** Finalize shared understanding.

Confirm:
- Core behaviors are understood
- Major risks are visible
- Boundaries are reasonable
- The model is “good enough” to proceed

Declare readiness for architecture and backlog shaping.

---

## Section 2: Event Storming Checklist

- [ ] Ontology referenced consistently
- [ ] Core events identified
- [ ] Events expressed in past tense
- [ ] Commands and actors identified
- [ ] Policies and decisions surfaced
- [ ] Boundaries visible
- [ ] Out‑of‑scope behaviors documented
- [ ] Ready for downstream design

---

## Section 3: Event Storming Capture Template

### Epic
- Name:
- Description:

### Domain Events

| Event | Description | Notes |
|------|-------------|-------|
|      |             |       |

### Commands

| Command | Actor | Resulting Event(s) | Notes |
|--------|-------|--------------------|-------|
|        |       |                    |       |

### Policies / Rules

| Policy | Triggering Event | Outcome | Notes |
|--------|------------------|---------|-------|
|        |                  |         |       |

### Boundaries / Responsibilities

| Boundary | Description | Notes |
|----------|-------------|-------|
|          |             |       |

### Out of Scope / Deferred

- 
- 

### Risks & Open Questions

- 
- 

### Approval

- Date:
- Participants:
- Notes: