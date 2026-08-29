# Agent Execution Feature

## Overview
The reference agent-execution feature accepts a chat message, applies governance, invokes a Microsoft Agent Framework agent, optionally routes tools through typed application requests, and persists the resulting conversation.

## Acceptance Criteria
- [ ] Every model invocation is governed before it runs.
- [ ] User and assistant messages remain ordered and tenant/owner scoped.
- [ ] Tools use the shared scoped execution gateway rather than direct persistence access.
- [ ] Suggested prompts and follow-up actions reuse the normal message submission flow.
- [ ] Tool descriptions define intended use and side effects.

## Out of Scope
- Long-running workflow orchestration.
- Domain-specific scoring and evaluation schemas.
- Cross-service agent coordination.