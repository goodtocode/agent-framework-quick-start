@playbookRepeatability
Feature: Playbook repeatability (Rerun, Recall, Replay)
As a playbook host
I repeat a previous SQL Statistics execution in a chosen mode
Then Governance decides which stages run and Playbook runs only those, so the result matches the mode's contract

Scenario Outline: Repeat a prior execution after the live database changed
	Given a SQL statistics playbook is registered
	And the connected database reports a size of <originalGb> GB
	And I have rerun the SQL statistics playbook for database "<database>"
	And the connected database then reports a size of <liveGb> GB
	When I repeat the SQL statistics playbook in "<mode>" mode
	Then the repeat reports mode "<mode>"
	And the repeat evaluation reports "<classification>"
	And the repeat is linked to its source "<linked>"
	And the repeat reuses the recorded collect input "<database>" "<reusesInput>"

Examples:
	| database | originalGb | liveGb | mode   | classification | linked | reusesInput |
	| Contoso  | 50         | 500    | Rerun  | Large          | false  | false       |
	| Contoso  | 50         | 500    | Replay | Medium         | true   | true        |
	| Contoso  | 50         | 500    | Recall | Medium         | true   | true        |

Scenario Outline: Repeat without a prior execution is rejected
	Given a SQL statistics playbook is registered
	When I repeat the SQL statistics playbook in "<mode>" mode
	Then the repeat is rejected as "<rejection>"

Examples:
	| mode   | rejection |
	| Recall | NotFound  |
	| Replay | NotFound  |
