# Blazor Project Layout Guide

## Purpose

This guide defines the standard folder organization for Blazor applications.

The goals are:

- Feature-oriented organization
- Functional cohesion
- Clear ownership
- Consistent developer experience
- Simplicity over architectural theory
- Easy extraction of reusable libraries
- Easy onboarding for new developers

The guiding principle is:

> Organize by ownership and purpose, not by technical artifact type.

A developer should be able to determine where a file belongs by answering:

- Is it a business capability?
- Is it reusable UI?
- Is it application infrastructure?
- Is it part of the application shell?

---

# Top-Level Structure

```text
Presentation.Web
│
├── Features
├── Library
├── Infrastructure
├── Shell
├── Properties
├── wwwroot
│
├── Program.cs
├── ConfigureServices.cs
└── ...
```

---

# Features

Features contain user-facing business capabilities.

A feature represents functionality that could disappear entirely in a different application.

Examples:

```text
Chat
Assets
Attestations
Users
Orders
Reports
```

Each feature owns its own:

- Page
- Components
- Models
- Services

Nothing leaves a feature until reuse has been proven.

## Structure

```text
Features
│
├── HomePage.razor
│
├── Chat
│   ├── ChatPage.razor
│   ├── Components
│   ├── Models
│   └── Services
│
├── Assets
│   ├── AssetPage.razor
│   ├── Components
│   ├── Models
│   └── Services
│
├── Attestations
│   ├── AttestationsPage.razor
│   ├── Components
│   ├── Models
│   └── Services
```

## Rules

- Features own their components.
- Features own their models.
- Features own their services.
- Features own their state.
- Features should remain independently understandable.
- If a feature is deleted, everything required by that feature should disappear with it.

---

# Library

Library contains reusable application components that are neither business features nor application shell concerns.

Library content should be portable between applications and suitable for future extraction into one or more Razor Class Libraries.

Typical examples include:

```text
Authentication Components
Skeleton Components
Wizard Components
Form Components
Reusable Workflows
```

Each library item follows the same functional cohesion pattern as features.

## Structure

```text
Library
│
├── FormChangeType.cs
│
├── Auth
│   ├── AuthComponent.razor
│   ├── Components
│   ├── Models
│   └── Services
│
├── Skeleton
│   ├── SkeletonComponent.razor
│   ├── Components
│   ├── Models
│   └── Services
│
├── Wizard
│   ├── WizardComponent.razor
│   ├── Components
│   ├── Models
│   └── Services
```

## Rules

- Library items should not depend on individual features.
- Library items should be reusable across multiple applications.
- Library items should be candidates for future Razor Class Libraries.
- Library items should not contain application shell concerns.
- Library items should not contain business-specific functionality.

---

# Infrastructure

Infrastructure contains technical implementation details required by the application.

Infrastructure supports features, library components, and the application itself.

Infrastructure is not business functionality.

Infrastructure is not reusable UI.

## Structure

```text
Infrastructure
│
├── Clients
├── Http
├── Options
├── Storage
```

### Clients

Contains generated or handwritten API clients.

Examples:

```text
Infrastructure
└── Clients
    ├── BackendApiClient.g.cs
    └── UserApiClient.cs
```

### Http

Contains HTTP-related implementation services.

Examples:

```text
Infrastructure
└── Http
    └── ApiService.cs
```

### Storage

Contains browser and application storage functionality.

Examples:

```text
Infrastructure
└── Storage
    └── LocalStorageService.cs
```

### Options

Contains configuration classes used by the Options Pattern.

Examples:

```text
Infrastructure
└── Options
    ├── BackendApiOptions.cs
    └── ResilientHttpClientOptions.cs
```

## Rules

- Infrastructure contains technical services.
- Infrastructure should not contain business workflows.
- Infrastructure should not contain pages.
- Infrastructure should not contain user-facing features.
- Infrastructure should not become a dumping ground for miscellaneous code.

---

# Shell

Shell contains the application host.

Shell remains even if every feature is removed.

Shell establishes how the application operates and renders.

Examples:

```text
App.razor
Routes.razor
MainLayout.razor
Navigation
Error Pages
```

## Structure

```text
Shell
│
├── Layout
│   ├── MainLayout.razor
│   ├── Error.razor
│   └── NotFound.razor
│
├── _Imports.razor
├── App.razor
└── Routes.razor
```

## Rules

- Shell owns application composition.
- Shell owns routing.
- Shell owns layout.
- Shell owns navigation structure.
- Shell should not contain business functionality.
- Shell should not contain feature-specific behavior.

---

# Properties

```text
Properties
```

Contains standard .NET project configuration artifacts such as:

```text
launchSettings.json
serviceDependencies.json
```

---

# wwwroot

```text
wwwroot
```

Contains static application assets.

Examples:

```text
css
images
icons
fonts
```

---

# File Placement Decision Tree

## Is it a business capability?

```text
Features
```

Examples:

```text
Chat
Assets
Attestations
Orders
Reports
```

---

## Is it reusable UI or reusable application functionality?

```text
Library
```

Examples:

```text
Wizard
Authentication Components
Skeleton Views
Form Controls
```

---

## Is it a technical implementation detail?

```text
Infrastructure
```

Examples:

```text
HTTP
Storage
Options
API Clients
```

---

## Is it required for the application host itself?

```text
Shell
```

Examples:

```text
App.razor
Routes.razor
MainLayout.razor
Error Pages
```

---

# Guiding Principles

1. Organize by ownership.
2. Prefer functional cohesion.
3. Keep features self-contained.
4. Keep technical concerns in Infrastructure.
5. Keep reusable UI in Library.
6. Keep application composition in Shell.
7. Extract to shared libraries only when reuse is proven.
8. Favor clarity over architectural purity.

When uncertain:

```text
Could this disappear in another application?
```

If yes:

```text
Features
```

If no, determine whether it is:

```text
Reusable UI      → Library
Technical code   → Infrastructure
Application host → Shell
```