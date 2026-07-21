# User Flows

## Purpose
Authoritative source for user journeys and expected behavior paths.

## Usage Rules
- Use ontology terms consistently.
- Keep flows implementation-agnostic.
- Capture success, alternate, and error paths explicitly.

## Actor: Operations User

### Entry Points
- Sign in and open dashboard.
- Open asset detail page.
- Review AI-generated recommendations.

### Success Path
1. User selects an asset.
2. System loads status, classifications, and recommendations.
3. User confirms an action.
4. System creates or updates a work item and records outcome.

### Alternate Paths
- User defers recommendation and marks for later review.
- User updates recommendation parameters before execution.

### Error Paths
- Asset data unavailable: show recoverable error and retry option.
- Recommendation execution failure: return actionable ProblemDetails and audit event.

## Notes
- Use these flows to derive acceptance criteria in feature documents.
- Use event storming artifacts to validate transitions and policy decisions.
