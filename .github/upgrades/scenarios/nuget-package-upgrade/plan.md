# NuGet Package Upgrade Plan

## Overview

**Target**: Upgrade the Fluent UI Blazor component, Emoji, and Icons packages from v4.14.4 to v5.0.0 and migrate the Blazor Web UI to supported Fluent v5 APIs.
**Scope**: One Blazor WebAssembly project with shared UI-library components, application shell/theme styling, chat and version-management workflows.

## Tasks

### 01-upgrade-fluent-packages: Upgrade Fluent UI package references and hosting integration

Update `src/Presentation.Web/Presentation.Web.csproj` to use the unified v5.0.0 component, Emoji, and Icons packages. Migrate application-level registrations, stylesheet references, imports, and provider composition to Fluent v5 patterns, using the API-diff artifact to replace removed provider and runtime APIs rather than retaining compatibility workarounds.

**Done when**: All three Fluent references resolve to v5.0.0, the app uses supported v5 service/provider setup, and the Presentation.Web project restores without v4 Fluent dependencies.

---

### 02-migrate-shell-theme-navigation: Migrate theme, shell, navigation, and overlay behavior

Update the application shell and custom styling for Fluent v5 design-theme, color-token, navigation, dialog, and overlay changes. Preserve the existing persisted theme preference and verify that custom CSS uses supported v5 token and component surfaces rather than obsolete Fluent internals. Address `Appearance`, layout, and navigation API changes found in the shell and reusable menu components.

**Done when**: Light/dark theme selection, branding colors, navigation routes, dialog behavior, and shell responsiveness compile against v5 and use supported APIs.

---

### 03-migrate-ui-components: Migrate feature and shared Fluent component usage

Replace removed and signature-changed Fluent APIs across shared library components and feature pages, including buttons, stacks, cards, badges, text input, and icon usage. Preserve EditForm binding and validation behavior in chat and version-management workflows, and update markup/parameters to Fluent v5 equivalents identified by the full assessment and build output.

**Done when**: Feature and shared UI components compile without obsolete Fluent APIs, interactive inputs retain their bindings and validation behavior, and component styling does not depend on removed v4 APIs.

---

### 04-validate-fluent-v5-experience: Validate the Fluent v5 build and primary workflows

Build the solution warning-free and run the integration test project. Smoke-test the Blazor application in light and dark themes, covering navigation, dialogs, chat input and submission, authenticated entry points, and responsive shell behavior. The assessment found no `FluentDataGrid` usage, so DataGrid-specific testing is not required unless later build analysis finds a dependency.

**Done when**: The solution builds without errors or warnings, integration tests pass, no Fluent v4 references remain, and the documented primary UI smoke checks pass.
