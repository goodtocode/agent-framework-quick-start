# Tool Design and Implementation Governance

## Purpose
Define project-agnostic standards for AI tools that preserve Clean Architecture boundaries.

## Principles
- Tools translate conversational intent; application and domain layers own business behavior.
- Tool inputs and outputs are explicit, typed, stable, and suitable for model consumption.
- Authorization, tenant scope, ownership, validation, and error semantics flow through application handlers.
- Every tool invocation is observable and every mutation is auditable.

## Required Invocation Path

```text
Tool method -> scoped tool base -> application execution gateway -> mediator pipeline -> command/query handler -> domain or infrastructure
```

Tools must use a shared scoped execution pattern such as `ScopedAgentTool` and `IToolApplicationExecutor`. They must not access a `DbContext`, repository, ORM, SQL connection, or mediator directly.

## Tool Responsibilities
- Validate argument shape that the runtime cannot validate.
- Map an operation to a typed command or query.
- Invoke that request through the shared gateway.
- Return a clear, concise response without changing its underlying meaning.
- Describe purpose, scope, prerequisites, side effects, and intended use with `Description` attributes.

## Application Responsibilities
- Own use cases, invariants, authorization, and tenant/owner filtering.
- Return typed results and explicit validation, not-found, conflict, or forbidden outcomes.
- Apply governance and observability requirements before inference or side effects.

## Side Effects
- Read tools are side-effect free.
- Write tools require explicit user confirmation when the action is consequential.
- A successful write may return a compact follow-up action for the chat UI, but it must never bypass the normal user-message to assistant-message flow.

## Testing and Guardrails
- Unit test tool argument mapping and conversational result shaping.
- Integration test the command/query behavior, including authorization and RLS.
- Add architecture tests that reject direct persistence access, direct mediator resolution, direct scope creation outside the shared base, and direct `AITool` inheritance outside that base.

## Definition of Done
- Tool behavior is implemented through typed application requests.
- Tool descriptions tell the model when to call the tool and its scope.
- Side effects are explicit and auditable.
- Relevant handler, tool, and architecture tests pass.