# Copilot Instructions for Microsoft Agent Framework Quick-start

## Project Overview
- **Microsoft Agent Framework Quick-start** is a C# solution for AI-powered monitoring, classification, and mitigation of digital assets.
- Built on a clean architecture: ASP.NET Core Web API backend, Blazor WebAssembly frontend, SQL Server storage, and Microsoft Semantic Kernel for AI/LLM integration.
- Infrastructure is managed via Azure Bicep and deployed using GitHub Actions.

## Key Architectural Patterns
- **Frontend:** `src/Presentation.Web/` (Blazor WebAssembly)
- **Backend:** `src/Presentation.Api/` (ASP.NET Core Web API)
- **Core Logic:** `src/Core.Application/`, `src/Core.Domain/`
- **AI Integration:** `src/Infrastructure.AgentFramework/` (Semantic Kernel plugins, prompt orchestration)
- **Persistence:** `src/Infrastructure.SqlServer/` (SQL Server, migrations)
- **IaC:** `.azure` (Bicep), Azure Bicep in deployment scripts

## Developer Workflows
- **Build:** Use `dotnet build GoodToCode.AgentFramework.Web.slnx` in `/` to build the solution.
- **Test:** Integration gherkin specs in `src/Tests.Integration/`.
- **Run:** Launch via Visual Studio or `dotnet run` from solution root.
- **CI/CD:** Managed by GitHub Actions (`.github/workflows/`).
- **IaC Deploy:** See `gtc-agent-standalone-iac.yml` for infrastructure deployment.

## Naming & Conventions
- **C# code:** PascalCase for types/methods/properties, _camelCase for private fields, camelCase for locals.
- **Database:** PascalCase plural for tables, PascalCase for columns, `Id` for PK, `RelatedEntityId` for FK.
- **API:** Kebab-case, plural nouns for endpoints (e.g., `/api/chat-sessions`).
- **Files/Folders:** C# files match main class, folders use PascalCase, config/docs use lowercase-hyphens.
- See `docs/naming-conventions.md` for details.

## Integration & Extensibility
- **Microsoft Agent Framework:** Add plugins in `src/Infrastructure.AgentFramework/Plugins/`.
- **External Integrations:** Use `src/Core.Application/Common/` and `src/Infrastructure.AgentFramework/` for connectors.
- **RBAC & Security:** Enforced in API layer, see `ConfigureServicesAuth.cs`.

## References
- [README.md](../README.md): Project overview and getting started

## Examples
- To add a new agent plugin: create in `src/Infrastructure.AgentFramework/Plugins/`, register in `ConfigureServices.cs`.
- To add a new API endpoint: implement in `src/Presentation.Api/`, follow API naming conventions.

---
For further details, always check the referenced docs and existing code patterns in the relevant folders.