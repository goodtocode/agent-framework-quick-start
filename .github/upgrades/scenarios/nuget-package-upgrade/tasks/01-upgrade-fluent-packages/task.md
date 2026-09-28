# 01-upgrade-fluent-packages: Upgrade Fluent UI package references and hosting integration

Update `src/Presentation.Web/Presentation.Web.csproj` to use the unified v5.0.0 component, Emoji, and Icons packages. Migrate application-level registrations, stylesheet references, imports, and provider composition to Fluent v5 patterns, using the API-diff artifact to replace removed provider and runtime APIs rather than retaining compatibility workarounds.

**Done when**: All three Fluent references resolve to v5.0.0, the app uses supported v5 service/provider setup, and the Presentation.Web project restores without v4 Fluent dependencies.

## Research

### Scope Inventory
- **Project**: `src/Presentation.Web/Presentation.Web.csproj` is the only project that references the Fluent packages. `get_project_dependencies` confirmed package versions are defined directly in this SDK-style project; Central Package Management is not enabled.
- **Packages**: `Microsoft.FluentUI.AspNetCore.Components`, `.Emoji`, and `.Icons` move from 4.14.4 to the assessment-approved unified 5.0.0 version.
- **Application integration**: `Program.cs` already registers `AddFluentUIComponents()`. `Shell/App.razor` uses v4 loading-theme assets and `FluentDesignTheme`; `Shell/Layout/MainLayout.razor` registers separate menu and dialog providers.
- **v5 direction**: The v5 package setup uses the bundled component stylesheet and a top-level `FluentProviders` component. The migration removes the v4 loading-theme script and `FluentDesignTheme` from the application document. Detailed theme/token work remains in task 02.
- **Change signals**: The full assessment found 186 removed-API and 55 signature-change usages. This task is limited to package and root hosting integration; feature-level migrations remain in later tasks.
