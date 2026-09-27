@documentReviewPlaybook
Feature: Document Review CER playbook execution
As a playbook host
I execute the Document Review Collect, Evaluate, and Record stages deterministically
And I produce a valid governance record for each stage

Scenario Outline: Execute the Document Review playbook end to end
	Given I have a document review request with content "<document>"
	When I execute the document review playbook
	Then the review status is "<status>"
	And the review is approved is "<approved>"
	And 3 governance records are produced

Examples:
	| document         | status   | approved |
	| Document content | Approved | true     |
	|                  | Rejected | false    |
