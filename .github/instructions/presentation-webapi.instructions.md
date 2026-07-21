# Presentation Web API Instructions

## Scope
Applies to `src/Presentation.Api/`.

## Rules
- Expose stable REST endpoints with clear resource naming.
- Delegate business execution to Application layer.
- Use ProblemDetails for error responses.
- Keep API contracts explicit and version-friendly.
- Keep OpenAPI metadata accurate.

## Dependencies
- Allowed: Application services/handlers, shared contracts.
- Forbidden: business logic embedded in controllers/endpoints; direct SQL access.

## AI Output Expectations
- Validate inputs.
- Return correct status codes.
- Add/update API tests for endpoints and error paths.
