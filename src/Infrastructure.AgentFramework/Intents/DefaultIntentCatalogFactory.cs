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
        new IntentDefinition(IntentNames.SelectActor, Examples: [],
            Captures: [new PhraseCapture("select actor ", "id", CaptureKind.GuidDFormat)]),
        new IntentDefinition(IntentNames.SelectChatSession, Examples: [],
            Captures: [new PhraseCapture("select chat session ", "id", CaptureKind.GuidDFormat)]),

        new IntentDefinition(IntentNames.QueryChatSessionsForSelectedActor,
        [
            "query chat sessions for the selected actor",
            "list chat sessions for the selected actor",
            "show chat sessions for the selected actor"
        ]),

        new IntentDefinition(IntentNames.QueryChatMessagesForSelectedChatSession,
        [
            "query chat messages for the selected chat session",
            "list chat messages for the selected chat session",
            "show chat messages for the selected chat session"
        ]),

        // Mid-chain journey entry point: the customer can jump straight to their own messages for
        // the chat session they are already in / have selected, without first walking the
        // actor -> chat session chain. Listed before the broader QueryChatMessagesList so these
        // more specific phrasings win the substring match.
        new IntentDefinition(IntentNames.QueryMyChatMessagesForCurrentChatSession,
        [
            "list my messages for this chat session",
            "list my chat messages for this chat session",
            "show my messages for this chat session",
            "show my chat messages for this chat session",
            "list messages for this chat session",
            "show messages for this chat session",
            "list my messages in this conversation",
            "show my messages in this conversation",
            "what have i said in this chat session",
            "show this chat session's messages"
        ],
        TokenRule: new IntentTokenRule(
            AllOf: ["chat", "session", "message"],
            AnyOfGroups:
            [
                ["list", "show", "what"],
                ["my", "this", "conversation"]
            ])
        ),

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
            FollowUpExamples: ["find an actor by name"],
            TokenRule: new IntentTokenRule(
                AllOf: ["actor"],
                AnyOfGroups:
                [
                    ["find", "lookup", "search", "who"],
                    ["name"]
                ],
                Captures:
                [
                    new PhraseCapture("actor named ", "name", CaptureKind.Rest),
                    new PhraseCapture("name of ", "name", CaptureKind.Rest),
                    new PhraseCapture("by name ", "name", CaptureKind.Rest)
                ])),

        new IntentDefinition(IntentNames.QueryActorsList,
        [
            "please list actors",
            "list actors",
            "show actors",
            "list all actors",
            "show all actors",
            "what actors do we have"
        ],
        TokenRule: new IntentTokenRule(
            AllOf: ["actor"],
            AnyOfGroups: [["list", "show", "what"]])
        ),

        new IntentDefinition(IntentNames.QueryMyActorsList,
        [
            "please list my actors",
            "list my actors",
            "show my actors",
            "what actors do i have"
        ],
        TokenRule: new IntentTokenRule(
            AllOf: ["actor", "my"],
            AnyOfGroups: [["list", "show", "what"]])
        ),

        new IntentDefinition(IntentNames.QueryChatSessionsList,
        [
            "list my chat sessions",
            "list my recent chat sessions",
            "list all of my recent chat sessions",
            "please list all of my recent chat sessions",
            "show all of my recent chat sessions",
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
        ],
        TokenRule: new IntentTokenRule(
            AllOf: ["chat", "session"],
            AnyOfGroups: [["list", "show", "what", "history", "conversation"]],
            NoneOf: ["message"])
        ),

        new IntentDefinition(IntentNames.QueryChatMessagesList,
        [
            "show my recent messages",
            "show recent messages across all my chat sessions",
            "what have i said recently",
            "show my message history"
        ],
        TokenRule: new IntentTokenRule(
            AllOf: ["message"],
            AnyOfGroups: [["list", "show", "what", "history"]],
            NoneOf: ["session"])
        ),

        new IntentDefinition(IntentNames.QueryChatMessagesForSession, Examples: [],
            Captures:
            [
                new PhraseCapture("show messages for chat session ", "sessionId", CaptureKind.GuidDFormat),
                new PhraseCapture("conversation history for chat session ", "sessionId", CaptureKind.GuidDFormat),
                new PhraseCapture("messages in chat session ", "sessionId", CaptureKind.GuidDFormat)
            ]),

        // Level 4 deterministic routing for web search: guarantee the "search the web for [query]"
        // phrasing never falls through to the LLM's own tool-selection. The Capture extracts the
        // query part so it can be passed to the search tool.
        new IntentDefinition(IntentNames.SearchWeb, Examples: [],
            Captures: [new PhraseCapture("search the web for", "query", CaptureKind.Rest)])
    ]);
}
