# Agent Governance Principles

## Purpose
Establish a portable governance baseline for inference-driven applications, independent of model provider, agent framework, or product domain.

## Mandatory Principles
1. **Observability**: every inference has a trace or correlation identifier and relevant evidence references.
2. **Auditability**: the acting principal, tenant, model, and available tool capabilities are attributable.
3. **Defensibility**: policy, justification context, and confidence assumptions are explicit.
4. **Repeatability**: prompt and input baselines are hashed so equivalent runs and drift can be distinguished.

## Runtime Boundary
Governance is a precondition for every inference action. Construct a typed governance record, enforce it, and send the resulting system instruction and metadata to the model runtime. When enforcement fails, do not invoke the model.

```text
Request -> governance record -> governance enforcement -> governed prompt context -> agent/model invocation
```

## Baseline Record
Each governed operation records:
- policy-profile version;
- trace and correlation identifiers;
- evidence references;
- owner, tenant, and principal display information;
- model reference and version;
- available or invoked tool references;
- applied policies and justification references;
- raw prompt and typed replay inputs for deterministic hashes.

## Profiles and Drift
Policy profiles are versioned. A durable workflow may persist a deterministic profile hash and validate it before execution or replay. Simple stateless chat can retain the same metadata in execution telemetry without introducing persistent workflow entities.

## Optional Evaluation Output
Features that evaluate or score data should use typed output contracts containing score, confidence, criteria, evidence, justification, and audit trace. This is optional for conversational chat and general utility tools.

## Non-Bypass Rules
- Do not add model calls that skip governance enforcement.
- Do not replace typed governance metadata with unstructured prompt text.
- Do not report a governed outcome without its policy and evidence context.
- Do not claim deterministic replay unless model, configuration, prompt, and inputs are sufficiently captured.