# Style Guide

## Purpose
Define repository-wide documentation and collaboration style.

## Markdown Formatting
- Use clear headings with logical hierarchy.
- Keep sections concise and scannable.
- Prefer bullets and short paragraphs.

## Naming Conventions
- Files: lowercase-hyphen style for markdown.
- Feature docs: `docs/product/features/<feature-name>.md`.
- Keep names descriptive and stable.

## Folder Organization
- Governance: permanent guidance in `docs/governance/`.
- Product: evolving context in `docs/product/sprint-0/` and `docs/product/features/`.
- AI instructions: `.github/` and `.github/instructions/`.

## Documentation Expectations
- Prefer one authoritative source per topic.
- Avoid duplicate or conflicting guidance.
- Update affected docs when behavior or requirements change.

## GitHub Issue Style
- Bug reports require the problem, steps to reproduce, expected behavior, error page URL, and screenshot.
- Feature requests require the problem and requested outcome; contributors may add supporting context when useful.
- Maintainers expand accepted work into feature documentation only when implementation planning needs additional detail.

## Pull Request Style
- Summarize intent, changed layers, testing evidence, and follow-up risks.
