# Semantic Intent Classification

## Overview

Add embedding-based intent classification as a controlled second tier between deterministic rule matching and the general Microsoft Agent Framework (MAF) fallback. The feature improves recognition of novel user phrasing while preserving deterministic routing, typed tool boundaries, authorization, observability, and a disabled-by-default rollout.

This feature applies to both template-derived products:

- `crucible-web`
- `agent-framework-quick-start`

The implementations share the same contracts and behavior. Their embedding initialization triggers differ because Crucible has a StateOfYour business-seeding workflow while the quick-start application owns its startup lifecycle.

## Business Problem

Exact phrase matching is fast and reliable but misses requests expressed in unfamiliar language. Those requests fall through to the slower and more expensive agent inference path, even when the intended tool route is already known.

For example, a catalog may contain `list my pipelines`, while a user asks `could you show my pipeline work`. Semantic classification should recognize the same intent without requiring an ever-growing list of exact phrases.

## Business Value

- Improve tool-routing coverage for novel phrasing.
- Preserve the deterministic rule path for known phrases and parameters.
- Reduce unnecessary full-agent inference.
- Keep semantic behavior measurable and reversible.
- Share one architecture across products derived from the same template.

## User Story

As a chat user, I want natural variations of a supported request to reach the correct tool, so that I do not need to use one exact command phrase.

## Product Behavior

The routing order is:

```text
User message
    |
    v
Rule classifier
    |
    +--> Match ------------------------------> Deterministic route
    |
    +--> Miss and semantic disabled ---------> MAF fallback
    |
    +--> Miss and semantic enabled
            |
            v
        Generate query embedding
            |
            v
        Search intent embedding store
            |
            +--> Valid non-parameterized intent -> Deterministic route
            |
            +--> No confident match ------------> MAF fallback
```

Semantic infrastructure failures must return no semantic match and allow the normal MAF fallback. They must not break the chat request.

## Current Scope

Semantic matching currently supports non-parameterized intents. These intents can be routed from an intent name alone, such as:

- List actors.
- List pipelines.
- Show recent messages.
- List playbooks.

Parameterized intents remain rule-based until a separate capture-resolution design is implemented. Examples include selecting a pipeline by ID, searching for a supplied query, or retrieving an entity by name. The follow-up-message chat cycle remains the supported way to collect missing parameters.

## Shared Contracts

The following contracts are shared by both products:

- `Embedding`
- `EmbeddingSource`
- `EmbeddingMatch`
- `IEmbeddingGenerator`
- `IIntentEmbeddingStore`
- `IIntentClassifier.ClassifyAsync(...)`
- `IntentClassificationOptions`

`IntentDefinition.Examples` is the canonical source. Stored embeddings are a regenerated index and must not become a second source of truth.

## Configuration

The API configuration exposes the following section in every environment file:

```json
"IntentClassification": {
  "EnableSemantic": false,
  "SemanticThreshold": 0.75,
  "TopKResults": 5
}
```

The section is bound through `IOptions<IntentClassificationOptions>`. `EnableSemantic` must remain `false` until evaluation and rollout approval are complete.

Azure embedding settings are bound through `AzureOpenAIOptions`, including `EmbeddingDeploymentName` with the default deployment name `embedding-fast`.

## Shared Implementation

### Rule classifier

`RuleIntentClassifier` remains the first tier. It handles exact examples, parameterized captures, and follow-up examples. Its contract is asynchronous so the routing pipeline can call both rule and semantic implementations consistently.

### Semantic classifier

`SemanticIntentClassifier`:

1. Rejects empty messages.
2. Generates a query vector through `IEmbeddingGenerator`.
3. Searches `IIntentEmbeddingStore` with the configured threshold and top-K value.
4. Resolves the returned intent name against the current `IntentCatalog`.
5. Skips parameterized intents that require captures.
6. Returns the first valid catalog match.
7. Logs the selected intent and similarity score.

### Hybrid classifier

`HybridIntentClassifier`:

1. Runs the rule classifier first.
2. Returns the rule match immediately.
3. Checks `EnableSemantic` after a rule miss.
4. Runs semantic classification only when enabled.
5. Returns `null` on non-cancellation semantic failures so MAF can handle the request.

### Storage

`SqlIntentEmbeddingStore` stores example vectors in the `Chat.IntentEmbeddings` table and performs cosine-similarity search. The store is behind `IIntentEmbeddingStore` so a future vector backend can replace SQL without changing the classifier contract.

## Project Implementations

### crucible-web

Crucible seeds embeddings through `Seed.StateOfYour`:

1. `StateOfYourSqlServerRuntimeScenario` invokes the seeder during product-data setup.
2. The seeder exits without work when semantic classification is disabled.
3. When enabled, it reads the current `IntentCatalog`.
4. It batches each intent's examples through the configured Azure embedding deployment.
5. It upserts the generated vectors into the Crucible SQL database.
6. Non-cancellation failures are logged and do not disable rule-based execution.

This path is intended for the StateOfYour SQL-backed business scenario and requires a real SQL connection and embedding deployment when enabled.

### agent-framework-quick-start

The quick-start application seeds embeddings through `IntentEmbeddingInitializationService`:

1. The hosted service starts with the application lifecycle.
2. It exits immediately when semantic classification is disabled.
3. It checks `IIntentEmbeddingStore.IsReadyAsync()` before generating vectors.
4. It reads the current `IntentCatalog` and batches examples.
5. It upserts vectors into the configured SQL database.
6. Initialization failures are logged without failing application startup.

This path is idempotent at startup through the store readiness check.

## Data Requirements

The SQL migration creates `Chat.IntentEmbeddings` with:

- Intent name.
- Embedding source.
- Original source text.
- Serialized vector.
- Weight.
- Created and updated timestamps.

The migration is generated but must be applied through the normal deployment process. Application startup must not apply migrations.

## Security and Governance

- Semantic classification must respect the same owner, tenant, and authorization boundaries as deterministic routing.
- Embedding vectors and source text are governed application data and must not contain secrets or credentials.
- Logs must record routing decisions and scores without recording sensitive message content unnecessarily.
- The feature switch provides an immediate rollback to deterministic-only behavior.
- Azure credentials must come from approved configuration or secret stores and must never be committed to source or test fixtures.

## Testing Requirements

Both repositories require:

- Generator tests with deterministic HTTP responses.
- Store tests for upsert, replacement, search, threshold filtering, ordering, deletion, readiness, and cosine similarity.
- Classifier tests for empty input, semantic match, rule-first behavior, disabled behavior, and provider failure fallback.
- Reqnroll scenarios covering the disabled switch and an enabled semantic match using deterministic embeddings and in-memory storage.
- API configuration parsing tests or equivalent validation confirming the switch is present and disabled by default.

Live Azure-backed seeding is a separate environment validation. It requires a configured `embedding-fast` deployment, SQL database, credentials, and an applied migration.

## Rollout

1. Deploy the infrastructure with semantic classification disabled.
2. Run the offline accuracy and latency evaluation.
3. Validate live seeding in an approved environment.
4. Tune the threshold and top-K settings.
5. Run the end-to-end regression suite.
6. Enable semantic classification only after accuracy, false-positive, latency, and cost targets are approved.
7. Monitor rule, semantic, and MAF fallback rates with rollback available.

## Out Of Scope

- Semantic extraction of captures for parameterized intents.
- Playbook evaluation metric embeddings.
- Azure AI Search migration.
- Automatic production enablement without evaluation.
- A second source of truth for intent definitions.
- Migration application during application startup.

## Definition Of Done

- [ ] Shared contracts and behavior are implemented in both products.
- [ ] Both seeding strategies are implemented and feature-flagged.
- [ ] Deterministic, semantic, fallback, and failure paths are tested.
- [ ] Live Azure and SQL validation is completed in an approved environment.
- [ ] Phase 8.3 accuracy, latency, cost, and false-positive targets are documented.
- [ ] Production enablement is approved and reversible.
- [ ] Governance documentation contains durable principles only.
- [ ] Tactical implementation details remain in this product feature document.
