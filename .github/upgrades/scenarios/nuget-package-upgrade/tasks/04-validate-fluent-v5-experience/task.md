# 04-validate-fluent-v5-experience: Validate the Fluent v5 build and primary workflows

Build the solution warning-free and run the integration test project. Smoke-test the Blazor application in light and dark themes, covering navigation, dialogs, chat input and submission, authenticated entry points, and responsive shell behavior. The assessment found no `FluentDataGrid` usage, so DataGrid-specific testing is not required unless later build analysis finds a dependency.

**Done when**: The solution builds without errors or warnings, integration tests pass, no Fluent v4 references remain, and the documented primary UI smoke checks pass.
