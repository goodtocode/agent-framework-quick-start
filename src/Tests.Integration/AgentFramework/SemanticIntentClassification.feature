@semanticIntentClassification
Feature: Semantic intent classification
As a chat routing system
I classify unfamiliar phrasing only when semantic classification is enabled

Scenario Outline: Hybrid classification honors the semantic feature switch
	Given the semantic classifier switch is "<enabled>"
	And the catalog contains intent "list-actors" with example "show actors"
	And the semantic example "show actors" is seeded for intent "list-actors"
	When I classify the message "<message>"
	Then the classified intent is "<expected>"

Examples:
	| enabled | message                         | expected    |
	| false   | could you show me the people    | none        |
	| true    | could you show me the people    | list-actors |
