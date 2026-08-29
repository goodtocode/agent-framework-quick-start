# Agent Runtime Observability

## Purpose
Define the minimum evidence needed to diagnose, audit, and improve an agent operation.

## Correlation
Generate one correlation ID for each user-initiated agent run. Propagate it to inference, tool calls, logs, outbound requests, and persisted run records when the product has them.

## Minimum Events
- Agent run started and completed or failed.
- Governance enforcement outcome and policy-profile version.
- Model reference and version.
- Tool name, operation, duration, and outcome.
- Validation, authorization, and external-provider failures.

## Data Handling
Log identifiers, durations, outcome codes, and metadata by default. Treat prompt, tool arguments, model responses, credentials, and customer data as sensitive; redact, hash, or persist them only under an explicit data-retention policy.

## Replay Evidence
Capture the prompt hash, input hash, model/configuration identity, tool capability version, and policy-profile version. These fields establish whether a later run is comparable to the original.