# Journey Roadmap

## Purpose

Provide a high-level map of the supported and anticipated user journeys for the Quick Start.

Individual feature definitions supply the detailed acceptance criteria and implementation scope.

---

## Journey 1 - Governed Chat

**Purpose:** Start, resume, and review a tenant- and owner-scoped conversation.

**Current Status:** Baseline implemented.

**Primary Outputs:** Ordered user and assistant messages, governed system instruction, and persisted chat session.

---

## Journey 2 - Scoped Tool Assistance

**Purpose:** Let the agent read relevant application data or approved external information through typed, scoped tools.

**Current Status:** Baseline implemented.

**Primary Outputs:** Tool-shaped conversational result with application validation and access control preserved.

---

## Journey 3 - Confirmed Tool Action

**Purpose:** Let a user explicitly confirm a consequential tool write and continue with an appropriate follow-up action.

**Current Status:** Baseline implemented.

**Primary Outputs:** Auditable application mutation and optional follow-up action submitted through normal chat flow.

---

## Journey 4 - Runtime Operations

**Purpose:** Review agent runs, tool outcomes, governance metadata, and replay evidence.

**Current Status:** Optional extension.

---

## Journey 5 - Domain Evaluation

**Purpose:** Evaluate domain-specific data with typed criteria, evidence, confidence, and recommendations.

**Current Status:** Optional extension.

---

## Baseline
1. Authenticate and provision the user-facing actor.
2. Start, resume, and review a chat session.
3. Submit a governed agent request.
4. Read tenant- and owner-scoped data through tools.
5. Confirm and perform an explicit write tool action.

## Optional Extensions
- Durable agent run history and replay.
- Agent and tool administration.
- Domain-specific evaluations and typed outcomes.
- Operational dashboards and diagnostics.