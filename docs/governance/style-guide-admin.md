# Administrative Application UX/UI Style Guide

## Scope
Apply this guide when the template is extended into an administration, operations, configuration, monitoring, or runtime-management application. It does not replace chat or consumer UX guidance.

## Principles
- Start with existing records and current state before creation forms.
- Make the selected record the center of the working context.
- Progress from collection to selection, details, relationships, history, diagnostics, then actions.
- Show provenance: what happened, why, when, under which configuration, and by whom.
- Present context before mutation or execution.

## Standard Workspace Order

```text
Existing records -> selected record -> related records -> runtime/history -> actions -> create or edit forms
```

## Operational Questions
Every screen should help answer at least one of: What exists? What is selected? What changed? What ran? What failed? What is related? What should happen next?

## States
Design empty, loading, error, unauthorized, degraded, and success states explicitly. Preserve keyboard navigation, accessible labels, visible selected state, and status text.