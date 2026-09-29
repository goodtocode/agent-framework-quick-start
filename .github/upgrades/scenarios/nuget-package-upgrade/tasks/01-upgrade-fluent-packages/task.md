# 01-upgrade-fluent-packages: Upgrade Fluent UI package references and hosting integration

Update `src/Presentation.Web/Presentation.Web.csproj` to use the unified v5.0.0 component, Emoji, and Icons packages. Migrate application-level registrations, stylesheet references, imports, and provider composition to Fluent v5 patterns, using the API-diff artifact to replace removed provider and runtime APIs rather than retaining compatibility workarounds.

**Done when**: All three Fluent references resolve to v5.0.0, the app uses supported v5 service/provider setup, and the Presentation.Web project restores without v4 Fluent dependencies.
