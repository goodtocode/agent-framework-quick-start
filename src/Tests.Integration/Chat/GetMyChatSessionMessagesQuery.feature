@getMyChatSessionMessagesQuery
Feature: Get My Chat Session Messages Query
As a chat-session owner
When I request messages for a chat session
I receive only messages that I own within my tenant

Scenario: Get chat session messages
	Given I have a definition "<def>"
	And the requested session is "<requestedSession>"
	And messages in my chat session exist "<exists>"
	And messages in another owner's chat session exist "<otherOwnerExists>"
	When I get my chat session messages
	Then The response is "<result>"
	And If the response has validation issues I see the "<responseErrors>" in the response
	And The chat session message response count is "<count>"

Examples:
	| def                              | result     | responseErrors | requestedSession | exists | otherOwnerExists | count |
	| success current owner            | Success    |                | current          | true   | false            | 1     |
	| success excludes other owner     | Success    |                | current          | true   | true             | 1     |
	| success no messages for my session | Success  |                | other            | false  | true             | 0     |
	| bad request empty session id     | BadRequest | ChatSessionId  | empty            | false  | false            | 0     |