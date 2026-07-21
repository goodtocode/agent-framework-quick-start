# Sprint 0 Tools and Methods

## Purpose
Document methods used for discovery and domain understanding during Sprint 0.

## Recommended Practices
- Whiteboarding for collaborative context mapping.
- Event Storming for behavior and workflow discovery.
- Ubiquitous Language alignment across roles.
- Domain modeling for entities, aggregates, and boundaries.
- User journey mapping for end-to-end paths.
- Context mapping for integration and ownership boundaries.
- AI-assisted discovery to draft and refine artifacts.

## Proven Working Pattern (Loop ↔ Copilot ↔ Repo)
1. Conduct interviews in Loop using plain text, headings, and bullets.
2. Ask Copilot to normalize language and structure markdown.
3. Request a single fenced code block containing the full artifact.
4. Copy the code block verbatim to avoid formatting corruption.
5. Store final markdown in repo files as the canonical source.

## Tool Roles
- Loop: collaboration and interview capture.
- Copilot: transformation, cleanup, and structured markdown generation.
- Repo: source of truth for versioned artifacts.

## Usage Guidance
- Prefer lightweight artifacts over large specification sets.
- Capture decisions in repository markdown files.
- Keep methods practical and repeatable for future teams.
- Avoid round-tripping canonical repo markdown back into collaboration tools for editing.
