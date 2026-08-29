# Agent Capability Registry

## Purpose
Optional pattern for products that need a durable, typed catalog of available agents, tools, model configurations, or external capabilities.

## Guidance
- Use typed descriptors with stable IDs, version, display name, allowed tenants, required permissions, and input/output contract references.
- Validate registrations at startup or deployment.
- Keep capability resolution in Application or Infrastructure composition, never in UI markup.
- Do not introduce a registry for the baseline template until capabilities must be configured independently of code.