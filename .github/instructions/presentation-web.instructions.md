# Presentation Web (Blazor) Instructions

## Scope
Applies to `src/Presentation.Web/`.

## Rules
- Keep UI logic in components/pages; move business decisions to backend/application.
- Organize pages and components by feature where possible.
- Use Fluent UI components consistently when applicable.
- Ensure accessibility: labels, keyboard navigation, and semantic markup.
- Keep routing and authorization explicit.

## Dependencies
- Allowed: typed API clients, shared contracts, UI libraries.
- Forbidden: direct persistence access or domain rule duplication.

## AI Output Expectations
- Build responsive, accessible components.
- Surface validation and error states clearly.
- Add/update UI tests where project supports them.
