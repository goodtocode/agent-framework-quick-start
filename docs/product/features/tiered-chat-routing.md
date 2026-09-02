# Tiered Chat Routing

## Purpose

The template routes each chat message through the cheapest reliable mechanism before using an
open-ended model response. This prevents the agent from responding with an announcement that it
will retrieve data without completing the tool call in the current synchronous turn.

`Core.Application` calls only `IChatMessageRoutingService.ResolveReplyAsync`. It does not reference
MAF/MEAI types, intent matching, tool invocation, or chat formatting. Callers choose either
`ChatRoutingMode.Routed` (the default) or `ChatRoutingMode.Direct` for an explicit diagnostic
bypass to the open agent turn.

## Cascade

1. **Tier 1: deterministic intent routing.** `RuleIntentClassifier` evaluates the declarative
   `IntentCatalog`. A successful `IntentMatch` is dispatched by `IIntentRouter` to a handler that
   executes the appropriate typed application query and formats its current result. The
   `IntentMatch` constructor is internal, preventing callers outside the intent implementation
   from fabricating a route without classification.
2. **Tier 2: forced-tool inference.** Unmatched messages use the same `AIAgent`, instructions,
   and tool catalog with `ChatToolMode.RequireAny`. MAF performs its native function selection and
   typed argument binding. Exceptions, empty replies, or a provider that does not honor the mode
   are logged at Warning and fall through with no retry.
3. **Tier 3: open agent turn.** The same history is reused in the ordinary `_agent.RunAsync` call,
   with default `ChatToolMode.Auto`. This handles conversational requests and requests that are
   not tool-shaped.

There is intentionally no bespoke semantic-classifier or classification-pipeline abstraction.
Tier 2 uses MAF/MEAI's native tool calling, avoiding a duplicate private intent catalog and JSON
argument contract.

## Tool Policy

`AgentToolInstructions` in `Presentation.Api/appsettings*.json` holds a global preamble and an
ordered instruction entry for each demo tool: chat sessions, chat messages, actors, and web
search. `AgentInstructionsComposer` reads the current `IOptionsMonitor` value and supplies MAF's
single `ChatOptions.Instructions` string. The global policy forbids promises of future updates:
the agent reports only the current result and users send another message to check later.

## Extension and Tests

Add a deterministic phrase to `DefaultIntentCatalogFactory` only after a concrete reliability gap
is observed, and mirror it in the relevant tool description and configuration instruction. Keep
new tools in the same MAF tool catalog so Tier 2 and Tier 3 automatically see them.

`ChatMessageRoutingServiceTests` verifies the Tier 1 short-circuit, Tier 2 forced-tool path, and
Tier 2 failure fallback to Tier 3. Live-provider end-to-end tests remain necessary to measure real
model tool-selection reliability.