# Feature: Playbook Repeatability (Rerun / Recall / Replay)

## Overview
Every example playbook "run" endpoint can repeat a prior execution. Repeat semantics come from `Goodtocode.Agents.Governance`, executed through `Goodtocode.Agents.Playbook`. This app references only Playbook and uses both concepts directly, with no mappers.

## Behavior
| Mode | What runs | Source needed |
|------|-----------|---------------|
| Rerun | Collect, Evaluate, Record against live data | no |
| Replay | Evaluate and Record on the recorded evidence | yes |
| Recall | Nothing; the recorded result is returned | yes |

The source is the given `SourceExecutionId`, otherwise the user's latest execution of the playbook. A missing source returns Not Found; a source without recorded evidence/finding returns Conflict.

## Where
`PlaybookRepeatCoordinator` resolves the source and builds the `PlaybookExecutionRequest`; `PlaybookWorkflowGraphExecutor` plans actions through `IRepeatPolicy`.

## Tests
`src/Tests.Integration/Playbooks/PlaybookRepeatability.feature` (Reqnroll) and `PlaybookReplayTests.cs`.
