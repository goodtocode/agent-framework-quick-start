@getOurActorsByNameQuery
Feature: Get Our Actors By Name Query
As a tenant user
When I search actors by name
I receive only matching actors from my tenant

Scenario: Get actors by name
	Given I have a definition "<def>"
	And I search for actor name "<name>"
	And matching actors in my tenant exist "<exists>"
	And matching actors in another tenant exist "<otherTenantExists>"
	When I get actors by name
	Then The response is "<result>"
	And If the response has validation issues I see the "<responseErrors>" in the response
	And The actor search response count is "<count>"

Examples:
	| def                         | result     | responseErrors | name  | exists | otherTenantExists | count |
	| success current tenant      | Success    |                | Avery | true   | false             | 1     |
	| success excludes other tenant | Success  |                | Avery | true   | true              | 1     |
	| lowercase name is supported  | Success    |                | robert | true  | false             | 1     |
	| success no matching actors  | Success    |                | Avery | false  | true              | 0     |
	| bad request empty name      | BadRequest | Name           |       | false  | false             | 0     |