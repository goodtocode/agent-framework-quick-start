@sqlStatisticsPlaybook
Feature: SQL Statistics Classification CER playbook execution
As a playbook host
I execute the SQL Statistics Collect, Evaluate, and Record stages deterministically
And I classify the database size against a versioned rubric
And I produce a valid governance record for each stage

Scenario Outline: Execute the SQL Statistics playbook end to end
	Given the connected database reports a size of <sizeGb> GB
	When I execute the SQL statistics playbook for database "<database>"
	Then the SQL size classification is "<classification>"
	And 3 governance records are produced for the SQL statistics playbook

Examples:
	| database     | sizeGb | classification |
	| ContosoSmall | 5      | Small          |
	| ContosoMed   | 50     | Medium         |
	| ContosoLarge | 500    | Large          |
