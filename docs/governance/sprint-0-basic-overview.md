# Sprint 0 Methodology
**Foundational Design & Enablement for a New Epic**

---

## 1. Purpose

Sprint 0 establishes the **minimum viable foundation** required to deliver, deploy, and operate a new epic with confidence.

Sprint 0 is not a delivery sprint. It is a **design‑with‑intent sprint** whose purpose is to:
- Stabilize domain language
- Define system boundaries
- Establish delivery and operational scaffolding
- Enable downstream sprints to focus on feature delivery without friction

Sprint 0 is **strictly time‑boxed to two weeks**.

---

## 2. Definition of Sprint 0

Sprint 0 produces three categories of outcomes:

### Design Foundations
- Domain ontology (core concepts and relationships)
- Event storming outcomes (events, commands, policies)
- Identified bounded context(s)
- Explicit scope boundaries

### Delivery Scaffolding
- Source control repositories
- CI/CD pipelines
- Infrastructure‑as‑Code baseline
- Identity and access primitives

### Delivery Intent
- Epic goal and success criteria
- Major deliverable checkpoints
- Enabled backlog for subsequent sprints

Sprint 0 concludes when the epic is **ready to build incrementally and operate safely**.

---

## 3. Core Design Principle

> **Sprint 0 design proceeds from _meaning_ to _behavior_, not the reverse.**

This is achieved through:
1. Ontology (what exists)
2. Event storming (what happens)
3. Architecture and delivery scaffolding (how it runs)

---

## 4. Design Sequence

### 4.1 Ontology Definition (Foundational)

**Purpose:**  
Establish a shared, stable domain language for the epic.

**Activities:**
- Identify core domain concepts (entities, aggregates, key nouns)
- Define canonical terminology and meanings
- Capture relationships between concepts
- Document assumptions and invariants

**Questions Answered:**
- What exists in this domain?
- What are the non‑negotiable concepts?
- What language must not drift across teams or time?

**Artifact:**
- A documented domain ontology
- Stored alongside the codebase or design repository

---

### 4.2 Event Storming (Behavioral)

**Purpose:**  
Model how the system changes over time using the previously defined ontology.

**Pre‑condition:**  
Ontology must exist and be referenced during event storming.

**Activities:**
- Identify domain events using ontological nouns
- Identify commands and actors
- Identify policies, rules, and invariants
- Discover boundaries, integrations, and responsibilities

**Questions Answered:**
- What happens in the system?
- What causes change?
- What rules apply over time?

**Artifact:**
- Event storming model (events, commands, policies)
- Clearly mapped to ontological concepts

---

### 4.3 Bounded Context & Scope Definition

**Purpose:**  
Constrain the epic to what can converge and deliver independently.

**Activities:**
- Identify the bounded context(s) relevant to the epic
- Explicitly define:
  - In‑scope concepts and behaviors
  - Out‑of‑scope concepts and behaviors
- Record deferred concerns explicitly

**Artifact:**
- Bounded context definition with scope clarity

---

### 4.4 Architecture & Operational Baseline

**Purpose:**  
Ensure the epic can be deployed, secured, and operated.

**Activities:**
- Define execution model and hosting approach
- Establish observability and error handling baseline
- Identify key non‑functional requirements
- Define integration boundaries

**Artifact:**
- High‑level architectural outline
- Operational assumptions and constraints

---

### 4.5 Delivery Scaffolding

**Purpose:**  
Remove all mechanical friction for future delivery sprints.

**Activities:**
- Create repositories with baseline structure
- Establish CI/CD pipelines
- Implement Infrastructure‑as‑Code
- Configure identity and access components

**Artifact:**
- Deployable, version‑controlled system skeleton

---

### 4.6 Delivery Intent & Planning

**Purpose:**  
Align on what will be delivered next and how success is measured.

**Activities:**
- Define epic success criteria
- Identify major checkpoints or feature slices
- Create an enabled, prioritized backlog

**Artifact:**
- Epic goal statement
- Sprint‑ready backlog

---

## 5. Exit Criteria

Sprint 0 is complete when:

- ✅ Domain language is documented and shared
- ✅ Event behavior is clearly understood
- ✅ Scope boundaries are explicit
- ✅ The system can be deployed end‑to‑end
- ✅ Ownership and operational responsibility are clear
- ✅ The next sprint can begin immediately

---

## 6. What Sprint 0 Is Not

Sprint 0 is explicitly **not**:
- A full architecture phase
- A requirements freeze
- A substitute for delivery
- A multi‑epic design exercise

---

## 7. Design Intent

**Ontology stabilizes meaning.**  
**Event storming reveals behavior.**  
Together, they ensure Sprint 0 produces foundations that scale across time, teams, and epics.