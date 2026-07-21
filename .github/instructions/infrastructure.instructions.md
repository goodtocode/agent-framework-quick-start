# Infrastructure Layer Instructions

## Scope
Applies to `src/Infrastructure.*` projects.

## Rules
- Implement external concerns: persistence, messaging, agent integrations, file/storage, and SDK adapters.
- Keep infrastructure behind application abstractions.
- Register dependencies in composition root only.
- Ensure resilience, retries, and observability for external calls.

## Dependencies
- Allowed: Application abstractions, external SDKs, EF Core, SQL providers.
- Forbidden: leaking infrastructure types into Domain.

## AI Output Expectations
- Keep adapters cohesive and replaceable.
- Include integration tests for critical infrastructure paths.
- Document any new external dependency and configuration.
