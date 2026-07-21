# Project Guide: Web API

## Purpose
Define standards for ASP.NET Core Web API implementation.

## Standards
- Use clear resource-oriented endpoint naming.
- Keep controllers/endpoints thin; delegate to Application layer.
- Maintain OpenAPI metadata and contract clarity.
- Use ProblemDetails for standardized errors.
- Keep DTOs and contracts explicit.
- Plan API versioning strategy when introducing breaking changes.
- Keep NSwag/OpenAPI generation aligned with contracts.

## Testing
- Add endpoint tests for success, validation, and error scenarios.
