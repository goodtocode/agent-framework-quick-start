# Ontology Definition — Conversation Guide
**Sprint 0 Foundational Design Artifact**

---

## Purpose

This document guides the collaborative creation of a **shared domain ontology** for a Sprint 0 epic.

Its goals are to:
- Establish stable domain language
- Remove ambiguity before behavioral modeling
- Create a durable semantic foundation for design and delivery

Ontology defines **what exists**, not what happens.

---

## Section 1: Conversation Guide

### Step 1 — Frame the Conversation

**Objective:** Align participants on purpose and limits.

Discuss and record:
- What this epic is fundamentally about
- What outcomes this ontology must support
- What is explicitly out of scope for this discussion

> Do not proceed if scope cannot be stated clearly.

---

### Step 2 — Noun Harvesting

**Objective:** Surface raw domain language.

Ask participants:
- “What things exist in this domain?”
- “What do we talk about when we discuss this problem?”

Rules:
- Capture nouns only
- Allow duplicates and synonyms
- Do not define or correct yet

Output: Unfiltered noun list.

---

### Step 3 — Concept Clarification

**Objective:** Turn nouns into candidate concepts.

For each noun:
- What does this mean in plain language?
- Is it distinct, or another name for something else?
- Is this a thing, role, relationship, or classification?

Capture ambiguity explicitly.

---

### Step 4 — Canonical Term Selection

**Objective:** Choose one term per concept.

Activities:
- Select a canonical name
- Record alternate or rejected terms
- Resolve conflicts through shared understanding, not hierarchy

This step establishes binding language.

---

### Step 5 — Relationship Definition

**Objective:** Define structural relationships.

For each concept:
- What does it contain?
- What does it reference?
- What does it depend on?

Rules:
- No events
- No lifecycles
- No ordering

---

### Step 6 — Boundaries & Invariants

**Objective:** Constrain meaning.

Identify:
- What must always be true
- What is intentionally excluded
- What concepts are deferred to future epics

Record assumptions and risks.

---

### Step 7 — Agreement & Closure

**Objective:** Finalize the ontology.

Confirm:
- Canonical language agreement
- Known gaps or open questions
- Rules for future changes

Declare ontology “stable enough” for event storming.

---

## Section 2: Ontology Checklist

- [ ] Epic scope stated and agreed
- [ ] Core nouns captured
- [ ] Each concept has a clear definition
- [ ] Canonical terms selected
- [ ] Relationships defined
- [ ] Invariants documented
- [ ] Out‑of‑scope items listed
- [ ] Ontology approved for downstream design

---

## Section 3: Ontology Capture Template

### Epic
- Name:
- Description:

### Canonical Concepts

| Concept | Definition | Notes |
|-------|------------|-------|
|       |            |       |

### Alternate / Rejected Terms

| Term | Canonical Concept | Reason |
|------|-------------------|--------|
|      |                   |        |

### Relationships

| Source Concept | Relationship | Target Concept | Notes |
|----------------|--------------|----------------|-------|
|                |              |                |       |

### Invariants

- 
- 

### Out of Scope / Deferred

- 
- 

### Open Questions

- 
- 

### Approval

- Date:
- Participants:
- Notes: