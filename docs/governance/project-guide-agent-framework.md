# Project Guide: Microsoft Agent Framework

## Purpose
Define standards for Agent Framework implementation and operations.

## Standards
- Register agents and dependencies through composition roots.
- Keep orchestration in Application workflows and infrastructure adapters.
- Implement tools according to [tool-design-and-implementation.md](tool-design-and-implementation.md): typed contracts, scoped execution gateway, application-owned policy, and controlled side effects.
- Enforce [agent-governance-principles.md](agent-governance-principles.md) before every inference action.
- Keep prompts versioned and discoverable.
- Use memory intentionally; avoid hidden coupling.
- Keep dependency registration explicit for testability.

## Testing Approach
- Unit test tool adapters and orchestration decisions.
- Integration test end-to-end agent flows with representative scenarios.
