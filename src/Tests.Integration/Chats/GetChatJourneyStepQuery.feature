@getChatJourneyStepQuery
Feature: Get Chat Journey Step Query
As a chat user
When journey context is driven by deterministic chat routing
I can retrieve suggested prompts and action chips from a centralized journey step

Scenario: Top-level journey step returns default prompts and no actions
	Given I have an active chat journey session
	When I get the chat journey step for the active session
	Then the journey level is "None"
	And the suggested prompt "List my chat sessions" is returned
	And the suggested prompt "Find an actor by name" is returned
	And no journey action options are returned

Scenario: Actor list routing updates journey step with actor selection actions
	Given I have an active chat journey session
	And an actor named "Robert" "Good" exists in my tenant
	When I route the chat message "please list actors"
	And I persist the routed assistant response for the active session
	And I get the chat journey step for the active session
	Then the journey level is "ActorSelected"
	And the suggested prompt "Query chat sessions for the selected actor" is returned
	And a journey action option with kind "actor" is returned
	And the router did not run the AI agent

Scenario: My chat sessions routing updates journey step with chat session actions
	Given I have an active chat journey session
	And an additional chat session titled "Prior Session" exists for me
	When I route the chat message "List my recent chat sessions"
	And I persist the routed assistant response for the active session
	And I get the chat journey step for the active session
	Then the journey level is "MyChatSessionsListed"
	And the suggested prompt "List my messages for this chat session" is returned
	And a journey action option with kind "chatsession" is returned
	And the router did not run the AI agent

Scenario: My messages for current chat session sets selected-session journey level
	Given I have an active chat journey session
	When I route the chat message "List my messages for this chat session"
	And I persist the routed assistant response for the active session
	And I get the chat journey step for the active session
	Then the journey level is "ChatSessionSelected"
	And the suggested prompt "Query chat messages for the selected chat session" is returned
	And the router did not run the AI agent
