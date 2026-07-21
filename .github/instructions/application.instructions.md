# Application Layer Instructions

## Scope
Applies to `src/Core.Application/`.

## Rules
- Implement use cases with clear request/response boundaries.
- Orchestrate domain behavior; do not move core business rules out of domain.
- Prefer async APIs end-to-end for I/O operations.
- Use validation before command/query execution.
- Keep handlers focused, deterministic, and testable.

## Dependencies
- Allowed: Core.Domain abstractions/models and shared application contracts.
- Forbidden: direct dependencies on Presentation types or EF-specific details.

## AI Output Expectations
- Implement vertical slices per feature.
- Return clear error outcomes.
- Add unit/integration tests for handlers and pipelines.
