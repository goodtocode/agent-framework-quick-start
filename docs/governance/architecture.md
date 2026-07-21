# Architecture

## Purpose
This document is the primary architectural authority for the repository.

## Architecture Style
- Clean Architecture for dependency boundaries.
- Vertical Slice Architecture for feature implementation flow.
- Domain-Driven Design (DDD) for business modeling.

## Solution Structure
- **Domain (`src/Core.Domain/`)**: entities, value objects, aggregates, domain behavior.
- **Application (`src/Core.Application/`)**: use-case orchestration, commands/queries, contracts.
- **Infrastructure (`src/Infrastructure.*`)**: persistence, integrations, agent tooling, external systems.
- **Presentation.Web (`src/Presentation.Web/`)**: Blazor WebAssembly UI.
- **Presentation.Api (`src/Presentation.Api/`)**: HTTP API surface and composition root.
- **AI Integration (`src/Infrastructure.AgentFramework/`)**: Semantic Kernel plugins and prompt orchestration.
- **Persistence (`src/Infrastructure.SqlServer/`)**: SQL Server and migrations.
- **IaC (`.azure`, `.github/workflows/`)**: Bicep-based provisioning and deployment automation.

## Dependency Rules
### Allowed
- Presentation -> Application
- Infrastructure -> Application (+ Domain where required by contracts)
- Application -> Domain

### Forbidden
- Domain -> Application/Infrastructure/Presentation
- Application -> Presentation
- Presentation -> Infrastructure internals

## Agent Architecture
- Use Microsoft Agent Framework integrations in `src/Infrastructure.AgentFramework/`.
- Add plugins in `src/Infrastructure.AgentFramework/Plugins/` and register through service configuration.
- Keep orchestration in Application services and infrastructure adapters.
- Tool invocation should be explicit, observable, and testable.
- Design adapters for future tool and model replacement.

## Definition of Done (Architecture)
- New changes respect layer boundaries and dependency rules.
- Business behavior remains in Domain/Application, not UI or infrastructure details.
- Architecture decisions align with this document unless formally superseded.
