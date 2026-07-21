# Development Workflow

## Purpose
Define a practical implementation workflow for developers and AI agents.

## Standard Workflow
1. **Sprint 0 (Time-Boxed to Two Weeks)**
   - Stabilize language first (ontology), then model behavior (event storming).
   - Establish architecture and delivery scaffolding (repo, CI/CD, IaC, identity baseline).
2. **Feature Definition**
   - Author `docs/product/features/<feature-name>.md` with acceptance criteria.
   - Map feature language to Sprint 0 ontology and user flows.
3. **Feature Implementation**
   - Implement vertical slices across layers while respecting architecture.
4. **Testing**
   - Add/update unit, integration, and API tests as needed.
5. **Pull Request**
   - Include summary, tests, risks, and linked feature document.
6. **Deployment**
   - Validate CI/CD workflows and environment readiness.

## AI Agent Workflow Alignment
- Read governance first.
- Read Sprint 0 basic and step guides before product artifacts.
- Validate that ontology terms are stable before behavioral implementation.
- Read feature document.
- Implement the smallest complete slice.
- Validate build and tests before completion.
