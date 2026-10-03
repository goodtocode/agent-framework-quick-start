@taxonomyPlaybook
Feature: Taxonomy Extraction and Classification CER playbook execution
As a playbook host
I execute the Taxonomy Collect, Evaluate, and Record stages agentically via the MAF workflow graph
And I produce a valid governance record for each stage

Scenario: Execute the Taxonomy playbook end to end
	Given the model extracts terms "budget, forecast" from the taxonomy source text
	And the model classifies the extracted terms as category "Finance" with confidence 0.9 and rationale "Discusses budgeting."
	And the model summarizes the taxonomy classification as "Classified as Finance with high confidence."
	When I execute the taxonomy playbook for text "Quarterly budget forecast"
	Then the taxonomy category is "Finance"
	And 3 governance records are produced for the taxonomy playbook
