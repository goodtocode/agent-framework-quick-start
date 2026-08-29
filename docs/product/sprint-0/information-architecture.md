# Information Architecture

## Purpose

Define the navigation, screen hierarchy, workspace boundaries, and information priorities for the Quick Start.

This document bridges user journeys, application surfaces, components, feature definitions, and implementation. It precedes wireframes and component work.

---

## Information Architecture Principles

### Journey-Centric

Navigation is organized around user outcomes, not storage entities or implementation details. Chat is the baseline journey; administration is introduced only when users must manage runtime or configuration records.

### Conversation-Centric

The active conversation is the primary working context. Session selection, transcript, prompts, follow-up actions, and input work together as one surface.

### Progressive Disclosure

Users move from entry point to session selection, conversation context, response, optional tool result, and next action. Details appear when required rather than occupying the primary chat workflow.

---

## Baseline Navigation

```text
Home

Chat

Optional Administration
```

## Baseline Surfaces
- **Home**: anonymous entry point.
- **Dashboard**: authenticated summary when the product requires one.
- **Chat**: conversation list, active transcript, suggested prompts, follow-up actions, and message input.
- **Administration**: optional record-first operational surfaces for configuration, agents, tools, and runtime history.

---

## Chat Workspace

The chat workspace answers:

```text
What conversation is active?

What has been said?

What can I ask or do next?
```

The chat transcript remains the primary context. Suggested prompts and follow-up actions sit adjacent to input controls, never inside or between persisted message bubbles.

---

## Administrative Extension

When operational screens are added, use record-first hierarchy:

```text
Existing records
	-> Selected record
		-> Related records
			-> Runtime and history
				-> Actions
					-> Create or edit forms
```

See [style-guide-admin.md](../../governance/style-guide-admin.md) for the conditional administration standard.