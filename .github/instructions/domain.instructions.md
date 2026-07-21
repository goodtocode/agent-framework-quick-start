# Domain Layer Instructions

## Scope
Applies to `src/Core.Domain/`.

## Rules
- Keep domain model free of infrastructure and UI concerns.
- Model business behavior with entities, value objects, aggregates, and domain events.
- Use ubiquitous language from `docs/product/sprint-0/ontology.md`.
- Enforce invariants inside domain types.
- Avoid anemic models where behavior belongs in the domain.

## Dependencies
- Allowed: .NET base libraries and other domain types.
- Forbidden: direct dependencies on Application, Infrastructure, Presentation, databases, HTTP, or external SDKs.

## AI Output Expectations
- Preserve aggregate boundaries.
- Avoid leaking persistence DTOs into domain.
- Add or update unit tests for domain behavior.
