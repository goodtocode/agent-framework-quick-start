@essayPlaybook
Feature: Essay Rubric Evaluation CER playbook execution
As a playbook host
I execute the Essay Collect and Record stages deterministically and the Evaluate stage agentically
And I produce a valid governance record for each stage

Scenario: Execute the Essay playbook end to end
	Given an essay submission chat message with content "This essay argues that renewable energy investment reduces long-term costs."
	And the model scores the essay as:
		| criterion                | score | reason                                |
		| Thesis clarity           | 0.9   | Clear thesis stated up front.         |
		| Evidence and support     | 0.8   | Some supporting detail.               |
		| Organization             | 0.85  | Logical flow.                         |
		| Grammar and mechanics    | 1.0   | No errors found.                      |
	When I execute the essay playbook for that submission
	Then the essay grade is "B"
	And 3 governance records are produced for the essay playbook
