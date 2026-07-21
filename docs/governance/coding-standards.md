# Coding Standards

## Purpose
Define implementation expectations for contributors and AI agents.

## Naming Standards
- Projects and folders: PascalCase.
- Namespaces: align with folder/project structure.
- Classes, records, enums: PascalCase.
- Interfaces: `I` prefix + PascalCase.
- Private fields: `_camelCase`.
- Locals and parameters: camelCase.
- API endpoints: kebab-case and plural nouns (for example `/api/chat-sessions`).
- Database objects: PascalCase plural tables, PascalCase columns, `Id` as primary key, `RelatedEntityId` as foreign key.

## Coding Conventions
- Use dependency injection; avoid service locators.
- Prefer async/await for I/O.
- Use structured logging with meaningful context.
- Validate inbound commands/requests consistently.
- Use explicit error handling and return stable outcomes.

## Testing Expectations
- Unit tests for domain and application logic.
- Integration tests for infrastructure and API behavior.
- API tests for contracts, status codes, and error paths.
- Use integration gherkin specifications in `src/Tests.Integration/` where applicable.

## API Standards
- RESTful resource-oriented endpoints.
- Maintain OpenAPI documentation.
- Use ProblemDetails for error responses.
- Keep DTOs/contracts explicit and version-aware.

## AI Expectations
Generated code must:
- Build successfully.
- Follow architecture boundaries.
- Follow naming and coding standards.
- Include or update relevant tests.
- Follow existing code patterns in the relevant folder before introducing new patterns.
