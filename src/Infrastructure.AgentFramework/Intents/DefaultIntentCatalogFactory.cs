namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// Builds the default <see cref="IntentCatalog"/> used in production: one <see cref="IntentDefinition"/>
/// per deterministic chat intent. This catalog guarantees known-good phrasings never fall through to the
/// LLM's own tool selection, which historically caused hallucinations like "I will fetch that for you."
/// </summary>
public static class DefaultIntentCatalogFactory
{
    public static IntentCatalog Create() => new(
    [
        // Parameterized selection intents evaluated first (RuleIntentClassifier checks Captures
        // before Examples across all intents, so ordering here only affects tie-breaks among captures).
        
        // Level 4 deterministic routing for actor-by-id lookup: guarantee this known-good
        // phrasing never falls through to the LLM's own tool-selection. Matches "actor {guid}"
        // anywhere in the message so phrasings like "get actor {id}", "find the actor whose id is {id}"
        // all resolve to the same deterministic route.
        IntentDefinitionFactory.ByIdKeyword(IntentNames.QueryActorById, "actor "),

        new IntentDefinition(IntentNames.QueryActorsByName,
            Examples: ["find an actor by name"],
            Captures:
            [
                new PhraseCapture("find an actor by name ", "name", CaptureKind.Rest),
                new PhraseCapture("find actor named ", "name", CaptureKind.Rest),
                new PhraseCapture("search actors for ", "name", CaptureKind.Rest),
                new PhraseCapture("look up a user called ", "name", CaptureKind.Rest),
                new PhraseCapture("who is actor ", "name", CaptureKind.Rest)
            ],
            FollowUpExamples: ["find an actor by name"]),

        new IntentDefinition(IntentNames.QueryActorsList,
        [
            "please list actors",
            "list actors",
            "show actors",
            "list all actors",
            "show all actors",
            "what actors do we have"
        ]),

        new IntentDefinition(IntentNames.QueryMyActorsList,
        [
            "please list my actors",
            "list my actors",
            "show my actors",
            "what actors do i have"
        ]),

        new IntentDefinition(IntentNames.QueryChatSessionsList,
        [
            "list my chat sessions",
            "list my recent chat sessions",
            "list my chats",
            "show my chat history",
            "show my recent chat sessions",
            "show recent conversations",
            "show my conversations",
            "what conversations have i had",
            "show previous chats",
            "list any chat sessions",
            "what have we talked about",
            "what have i asked you before"
        ]),

        new IntentDefinition(IntentNames.QueryChatMessagesList,
        [
            "show my recent messages",
            "show recent messages across all my chat sessions",
            "what have i said recently",
            "show my message history"
        ]),

        // Level 4 deterministic routing for web search: guarantee the "search the web for [query]"
        // phrasing never falls through to the LLM's own tool-selection. The Capture extracts the
        // query part so it can be passed to the search tool.
        new IntentDefinition(IntentNames.SearchWeb, Examples: [],
            Captures: [new PhraseCapture("search the web for", "query", CaptureKind.Rest)])
    ]);
}
