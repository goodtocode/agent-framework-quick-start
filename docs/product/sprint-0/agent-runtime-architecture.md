# Agent Runtime Architecture

## Purpose

Define the minimum runtime lifecycle for a governed, tool-enabled chat application.

This document is intentionally framework-oriented rather than domain-specific. It establishes a reusable execution pattern for future Agent Framework applications.

---

## Runtime Lifecycle

```text
Authenticate
	-> Select or create chat session
		-> Submit message
			-> Validate user and request
				-> Enforce governance
					-> Invoke agent
						-> Invoke scoped tools when required
							-> Persist ordered conversation outcome
								-> Refresh chat UI
```

1. An authenticated user submits a chat message.
2. The application validates tenant and owner context.
3. Governance produces a governed system instruction and execution metadata.
4. The agent receives the governed instruction and conversation history.
5. When needed, the agent invokes a scoped tool through the application execution gateway.
6. The application persists the user message and assistant result, then the UI refreshes the ordered conversation.

---

## Locked Baseline Invariants

- No model inference bypasses governance enforcement.
- No tool bypasses typed application commands or queries.
- `My` operations enforce owner and tenant scope; `Our` operations enforce tenant scope.
- The persisted conversation remains the source of truth for ordered user and assistant messages.
- Suggested prompts and follow-up actions invoke the normal submission path and do not inject synthetic transcript entries.
- Tool failures preserve application error semantics and never become success-shaped responses.

## Boundaries

The agent chooses whether to request a tool. Application handlers own authorization, business behavior, and persistence. The UI can offer suggested prompts and tool follow-up actions, but both must use the normal message submission path.

---

## Future Direction

Durable agent-run history, replay comparison, streaming, long-running workflows, and domain-specific evaluation outputs are optional extensions. They must preserve the baseline invariants above rather than introduce a parallel execution path.