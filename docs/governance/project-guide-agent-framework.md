# Project Guide: Microsoft Agent Framework

## Purpose
Define standards for Agent Framework implementation and operations.

## Standards
- Register agents and dependencies through composition roots.
- Keep orchestration in Application workflows and infrastructure adapters.
- Implement tools with clear contracts and controlled side effects.
- Keep prompts versioned and discoverable.
- Use memory intentionally; avoid hidden coupling.
- Keep dependency registration explicit for testability.

## Testing Approach
- Unit test tool adapters and orchestration decisions.
- Integration test end-to-end agent flows with representative scenarios.
