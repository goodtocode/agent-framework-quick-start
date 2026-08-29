# Copilot Instructions for Microsoft Agent Framework Quick-start

## Project Overview
- **Goodtocode Quick-start for Microsoft Agent Framework** is an open-source C# template for authenticated, governed, tool-enabled chat applications.
- Built on Clean Architecture: ASP.NET Core Web API backend, Blazor WebAssembly frontend, SQL Server storage, and Microsoft Agent Framework backed by Microsoft.Extensions.AI.
- Infrastructure is managed via Azure Bicep and deployed using GitHub Actions.

## Solution Layout
- **Frontend:** `src/Presentation.Web/` (Blazor WebAssembly)
- **Backend:** `src/Presentation.Api/` (ASP.NET Core Web API)
- **Core Logic:** `src/Core.Application/`, `src/Core.Domain/`
- **AI Integration:** `src/Infrastructure.AgentFramework/` (agent providers, scoped tools, and external integrations)
- **Persistence:** `src/Infrastructure.SqlServer/` (SQL Server, migrations)
- **IaC:** `.azure` (Bicep), Azure Bicep in deployment scripts

## Developer Workflows
- **Build:** Run `dotnet build Goodtocode.AgentFramework.Web.slnx` from the repository root.
- **Test:** Run `dotnet test src/Tests.Integration/Tests.Integration.csproj`; integration behavior uses Reqnroll Gherkin specifications.
- **Run:** Launch via Visual Studio or `dotnet run` from solution root.
- **CI/CD:** Managed by GitHub Actions (`.github/workflows/`).
- **IaC Deploy:** See `gtc-agent-standalone-iac.yml` for infrastructure deployment.

## Non-Negotiable Architecture
- Preserve dependency direction: Presentation -> Application -> Domain; Infrastructure depends on Application and Domain as required; Domain has no project dependencies.
- Use typed command/query requests through `Goodtocode.Mediator`; controllers and agent tools do not contain business behavior or direct persistence access.
- `My` requests filter by `OwnerId` and `TenantId`; `Our` requests filter by `TenantId`. Do not introduce authorization bypasses or test-auth backdoors.
- Persisted data uses EF Core and SQL Server. Create migrations when schema changes, but do not apply migrations at application startup.

## Agent, Tool, and Governance Baseline
- Place Agent Framework providers and tools in `src/Infrastructure.AgentFramework/Tools/` and register dependencies through composition roots.
- Tools use `ScopedAgentTool` and `IToolApplicationExecutor` to dispatch typed application requests. Tools must not access `DbContext`, repositories, SQL, or `ISender` directly.
- Every model inference must enforce governance before invoking the agent and must apply the governed system instruction at the runtime boundary.
- Use explicit `Description` attributes to state a tool's intent, scope, prerequisites, and side effects. Consequential writes require explicit user confirmation.

## Chat UX Baseline
- Keep the ordered persisted user/assistant bubble sequence as the source of truth for the conversation.
- Suggested prompts and tool follow-up actions must submit through the normal message-input path; never insert synthetic message bubbles.
- Keep assistant markdown rendering isolated from user plain-text rendering. Use Fluent UI components and avoid JavaScript interop except where an existing component requires it.

## Scoped and Detailed Guidance
- Apply the matching file-scoped rule in `.github/instructions/` when editing Application, Domain, Infrastructure, Presentation.Web, or Presentation.Api.
- Use `docs/governance/` for detailed architecture, coding, tool, governance, runtime-observability, and administrative UX guidance.
- Use `docs/product/` for product intent, ontology, user flows, feature definitions, and completion criteria.
- Follow existing patterns in the local folder before adding a new abstraction or dependency.