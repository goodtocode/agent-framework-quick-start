# Agent Runtime Architecture

## Lifecycle
1. An authenticated user submits a chat message.
2. The application validates tenant and owner context.
3. Governance produces a governed system instruction and execution metadata.
4. The agent receives the governed instruction and conversation history.
5. When needed, the agent invokes a scoped tool through the application execution gateway.
6. The application persists the user message and assistant result, then the UI refreshes the ordered conversation.

## Boundaries
The agent chooses whether to request a tool. Application handlers own authorization, business behavior, and persistence. The UI can offer suggested prompts and tool follow-up actions, but both must use the normal message submission path.