@tokenIntentClassification
Feature: Token intent classification
As a chat routing system
I classify resilient natural-language variants deterministically before semantic or LLM fallback

Scenario Outline: Token rules normalize aliases and punctuation
	Given the token catalog contains intent "<intent>" requiring all tokens "<allOf>" and any-of groups "<anyOfGroups>" with blockers "<noneOf>"
	When I classify the standalone token message "<message>"
	Then the token classified intent is "<expected>"

Examples:
	| intent          | allOf       | anyOfGroups | noneOf  | message                                  | expected        |
	| actor-by-name   | actor       | find,show;name,called |         | Find these ACTORS, called Avery!        | actor-by-name   |
	| actor-by-name   | actor       | find,show;name,called | message | Show an actor named Avery               | actor-by-name   |
	| actor-by-name   | actor       | find,show;name,called |         | Find this actor                          | none            |

Scenario Outline: Token rules extract values only after criteria match
	Given the token catalog contains intent "<intent>" requiring all tokens "<allOf>" and any-of groups "<anyOfGroups>" with capture prefix "<prefix>"
	When I classify the standalone token message "<message>"
	Then the token classified intent is "<expected>"
	And the token capture "<captureName>" is "<captureValue>"

Examples:
	| intent        | allOf | anyOfGroups | prefix             | message                                      | expected      | captureName | captureValue |
	| actor-by-name | actor | find;name   | find this actor by name of  | Find this actor by name of Ada Lovelace     | actor-by-name | name        | Ada Lovelace |
	| actor-by-name | actor | find;name   | find this actor by name of  | Find this actor without a subject           | none          | name        |             |

Scenario: More specific token rule wins
	Given the token catalog contains a broad intent "broad" with all tokens "actor" and any-of groups "show"
	And the token catalog contains a specific intent "specific" with all tokens "actor;my" and any-of groups "show"
	When I classify the standalone token message "show my actor"
	Then the token classified intent is "specific"

Scenario: Equal specificity token rules are ambiguous
	Given the token catalog contains a broad intent "first" with all tokens "actor" and any-of groups "find"
	And the token catalog contains a specific intent "second" with all tokens "actor" and any-of groups "find"
	When I classify the standalone token message "find actor"
	Then the token classified intent is "none"

Scenario Outline: Examples and follow-up rules remain deterministic fallbacks
	Given the token catalog contains intent "<intent>" with example "<example>" and follow-up "<followUp>"
	When I classify the token message "<message>" using prior user message "<priorMessage>"
	Then the token classified intent is "<expected>"
	And the token classification level is "<level>"

Examples:
	| intent       | example          | followUp       | message                 | priorMessage              | expected     | level |
	| list-chats   | list my chats    |                | Please list my chats    |                           | list-chats   | level1|
	| follow-up   |                  | which session? | session from yesterday  | which session?            | follow-up    | level1|
	| unknown      | known phrase     |                | unrelated request       |                           | none         | none  |
