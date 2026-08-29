# Coding Standards

## Purpose
Define implementation expectations for contributors and AI agents.

## Naming Standards
- Projects and folders: PascalCase.
- Namespaces: align with folder/project structure.
- Use plural folders for feature and collection boundaries, such as `Actors`, `Chats`, `Tools`, `Configurations`, and `Endpoints`.
- Name each source file after its primary type. Use singular or plural type names according to the type's responsibility, not mechanically: `ActorEntity.cs`, `ChatSession.cs`, `ChatSessionsQuery.cs`, and `ActorsTool.cs` are all valid.
- Do not create a file whose name duplicates a folder merely to satisfy a naming rule; use the type's precise responsibility in the filename.
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
- Convert wire or JSON inputs to typed contracts at the application boundary.
- Keep AI tool methods thin: map typed arguments to application requests and shape typed outcomes for conversation.
- Include correlation IDs and outcome metadata in agent and tool logs; never log credentials or sensitive prompt content by default.

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
- Use `Description` attributes to define a tool's intent, scope, prerequisites, and side effects.
- Add an architecture guard when a new tool boundary prohibits direct persistence or mediator access.
