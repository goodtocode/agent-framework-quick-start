# User Flows

## Purpose
Authoritative source for user journeys and expected behavior paths.

## Usage Rules
- Use ontology terms consistently.
- Keep flows implementation-agnostic.
- Capture success, alternate, and error paths explicitly.

## Actor: Authenticated User

### Entry Points
- Sign in and open chat.
- Start or select a chat session.
- Submit a message or choose a suggested prompt.

### Success Path
1. User submits a message.
2. System constructs governance context and invokes the agent.
3. Agent responds and may use a scoped tool through the application pipeline.
4. System persists and displays the ordered user and assistant messages.
5. When a write tool returns a follow-up action, the user can select it and the normal message flow continues.

### Alternate Paths
- User selects a suggested prompt to auto-send it through the normal chat input.
- Agent requests confirmation before invoking a consequential write tool.

### Error Paths
- Agent or tool failure: return a clear, recoverable error and retain the conversation context.
- Unauthorized or unavailable data: do not disclose protected data; return the application outcome.

## Notes
- Use these flows to derive acceptance criteria in feature documents.
- Use event storming artifacts to validate transitions and policy decisions.
