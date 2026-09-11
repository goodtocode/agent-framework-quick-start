# Pipeline Governance Principles (Static, Product-Wide)

## Purpose

Define a durable, reusable governance lock for pipeline systems with four mandatory principles:

1. Observability
2. Auditability
3. Defensibility
4. Repeatability

## Principle Definitions

- **Observability**: every execution path must be traceable with evidence references.
- **Auditability**: ownership/tenant and tool use must be attributable.
- **Defensibility**: applied policy and justification context must be preserved.
- **Repeatability**: the same inputs and configuration must be replayable and verifiable.

## Durable Lock Model

`GovernanceProfile` is the shared contract used across products:

- `PolicyProfileVersion`
- `ObservabilityRequired`
- `AuditabilityRequired`
- `DefensibilityRequired`
- `RepeatabilityRequired`
- deterministic `GovernanceLockHash` (SHA-256 of the profile)

The lock hash is used to detect drift/tampering and to verify cross-entity governance alignment.

## Governed Evaluation Output Schema (Required)

In addition to entity-level governance locks, evaluate-stage outputs must use a deterministic governed schema:

- `overall_score` (0-100)
- `overall_level`
- `overall_confidence` (0-1)
- `defensibility_summary`
- `criteria[]` with required fields:
  - `name`
  - `score` (0-100)
  - `level`
  - `justification`
  - `evidence`
  - `rubric_reference`
  - `confidence` (0-1)
  - `uncertainty_flag`
  - `defensibility`
- `strengths[]`
- `weaknesses[]`
- `recommendations[]`
- `audit_trace`:
  - `model_version`
  - `rubric_version`
  - `timestamp_utc`
  - `evaluation_id`

This schema is represented by `GovernedEvaluationOutputSchema` and validated at runtime before record-stage persistence.

## Hard-Persisted Fields

The following persisted entities must contain governance lock fields:

- `PipelineEntity`
  - `GovernancePolicyProfileVersion`
  - `GovernanceObservabilityRequired`
  - `GovernanceAuditabilityRequired`
  - `GovernanceDefensibilityRequired`
  - `GovernanceRepeatabilityRequired`
  - `GovernanceLockHash`
- `PlaybookEntity`
  - `GovernancePolicyProfileVersion`
  - `GovernanceObservabilityRequired`
  - `GovernanceAuditabilityRequired`
  - `GovernanceDefensibilityRequired`
  - `GovernanceRepeatabilityRequired`
  - `GovernanceLockHash`

## Non-Bypass Rules

- Pipeline and playbook creation/update must apply governance from authoritative pipeline kit registration.
- Governance flags must remain fully required for all four principles.
- Read and execute paths must validate governance hash integrity.
- Pipeline execution must reject playbooks whose governance profile/hash does not match pipeline governance.

## Reuse Guidance

This model is intentionally product-agnostic:

- Keep `GovernanceProfile` in shared core libraries.
- Allow each product to choose profile versions (for example `*.v1`, `*.v2`) while preserving hash semantics.
- Use the same lock profile contract for:
  - agent-framework quick starts
  - semantic-kernel quick starts
  - any orchestrated multi-step pipeline/runtime.
