# Development Workflow

## Purpose
Define a practical implementation workflow for developers and AI agents.

## Authoritative Developer-Agent Loop
This loop is the default workflow for every session and branch unless the developer explicitly overrides it.

1. **Design Outside Repository (Developer)**
   - Design in M365/Loop/Whiteboard.
   - Convert approved design intent into repository artifacts.
2. **Design Packet in Repository (Developer)**
   - Create or update product docs (for example `docs/product/features/<feature-name>.md`).
   - Include: problem, goals/non-goals, constraints, acceptance criteria, API/DB impact, rollout/risk notes.
3. **Design Readback and Gap Review (Agent)**
   - Agent reads the design packet and responds with:
     - intent summary,
     - ambiguity/risk list,
     - implementation plan and validation plan.
   - No code changes in this step.
4. **Design Loop (Developer + Agent)**
   - Developer refines docs; agent re-reviews until intent and plan are aligned.
5. **Issue and Branch Setup (Developer)**
   - Create or select GitHub issue.
   - Create or select branch.
6. **Implementation Pass (Agent)**
   - Implement only the approved scope.
   - Run targeted build/tests.
   - Stop with a clear summary of changed files, assumptions, and validation output.
7. **Graphical Review and Runtime Checks (Developer)**
   - Review diff, test status, and SQL/migration/runtime outcomes in VS Code/Visual Studio.
   - Run scripts (for example `reset-efnswag.ps1`) and execute additional checks as needed.
8. **Fix Loop (Developer + Agent)**
   - If issues are found, return to step 6 until clean.
9. **Commit, Push, PR (Developer by default)**
   - Developer performs commit/push/PR unless explicitly delegated.
   - Agent can assist with commit message, PR description, and follow-up fixes.
10. **CI and Merge**
   - Address CI feedback.
   - Merge only after approval and clean checks.

## Branch and Session Control Policy
1. **No Implicit Branch Creation by Agent**
   - Agent must not create branches unless explicitly asked.
2. **No Implicit Commit/Push by Agent**
   - Agent must not commit or push unless explicitly asked in the current session.
3. **Preferred Ownership**
   - Developer owns branch lifecycle and PR lifecycle by default.
4. **Conflict Resolution Rule**
   - On rebase conflicts, upstream is the source of truth; adapt session changes to upstream patterns.
5. **Transparency Requirement**
   - Agent always reports:
     - current branch name,
     - whether commit/push occurred,
     - exact verification commands run.

## Baseline Engineering Standards
- Read governance and relevant product docs before implementation.
- Validate ontology terms before behavior implementation.
- Implement smallest complete vertical slice.
- Validate build and tests before completion.
