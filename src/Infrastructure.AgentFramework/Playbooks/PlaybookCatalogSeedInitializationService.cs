using Goodtocode.AgentFramework.Core.Application.Playbooks;
using Goodtocode.AgentFramework.Core.Domain.Playbooks;
using Goodtocode.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks;

/// <summary>
/// Seeds the three built-in example Playbooks (SQL Statistics, Taxonomy, Essay) - their
/// identity plus each CER step's persisted processing/action - once at startup, idempotently,
/// following the same pattern as <see cref="Embeddings.IntentEmbeddingInitializationService"/>.
/// This is data seeding through the normal application command pipeline, not an EF migration, so
/// it respects the "never apply migrations at startup" rule while still guaranteeing every
/// Run*PlaybookCommand has a Playbook row to record its executions against.
/// </summary>
public sealed class PlaybookCatalogSeedInitializationService(
    IServiceScopeFactory scopeFactory,
    ILogger<PlaybookCatalogSeedInitializationService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            foreach (var playbook in BuiltInPlaybooks())
            {
                var existing = await sender.Send(new GetPlaybookQuery { Key = playbook.Key }, cancellationToken);
                if (existing is not null)
                {
                    continue;
                }

                await sender.Send(playbook, cancellationToken);
                logger.LogInformation("Seeded built-in playbook '{PlaybookKey}'.", playbook.Key);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Playbook catalog seeding failed; Run*Playbook commands may be unable to persist executions until it succeeds.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static IEnumerable<CreatePlaybookCommand> BuiltInPlaybooks()
    {
        yield return new CreatePlaybookCommand
        {
            Key = "sql-statistics",
            Name = "SQL Statistics Classification",
            Description = "Fully deterministic CER workflow: queries a database's size and object counts, classifies it against a size rubric, and records the classification. No LLM involvement.",
            WorkflowType = "Deterministic",
            Version = "v1",
            Collect = new PlaybookStepInput
            {
                Name = "Collect database statistics",
                Description = "Runs a deterministic T-SQL statistics query against the database named by the Collect-stage input string.",
                ActionFormat = PlaybookActionFormat.SqlQuery,
                ActionDefinition = """
                    DECLARE @db sysname = @databaseName;
                    IF DB_ID(@db) IS NULL THROW 50001, 'The specified database does not exist.', 1;
                    IF HAS_DBACCESS(@db) <> 1 THROW 50002, 'The configured credential does not have access to the specified database.', 1;
                    -- Executed dynamically against @db's catalog views:
                    SELECT
                        CAST((SELECT ISNULL(SUM(size), 0) FROM sys.database_files WHERE type IN (0, 1)) * 8.0 / 1024 / 1024 AS FLOAT) AS SizeGb,
                        (SELECT COUNT(*) FROM sys.tables) AS TableCount,
                        (SELECT COUNT(*) FROM sys.indexes WHERE index_id > 0) AS IndexCount;
                    """
            },
            Evaluate = new PlaybookStepInput
            {
                Name = "Classify database size",
                Description = "Classifies the collected SizeGb against named size bands (rubric sql-statistics.database-size v1).",
                ActionFormat = PlaybookActionFormat.Rubric,
                ActionDefinition = "Small: 0-20 GB; Medium: 20-100 GB; Large: 100+ GB. The collected SizeGb is matched to the first band whose Maximum it falls within."
            },
            Record = new PlaybookStepInput
            {
                Name = "Record classification",
                Description = "Projects the finding into a human-readable summary.",
                ActionFormat = PlaybookActionFormat.Template,
                ActionDefinition = "Database classified as {Classification} according to rubric {RubricVersion}."
            }
        };

        yield return new CreatePlaybookCommand
        {
            Key = "taxonomy",
            Name = "Taxonomy Extraction and Classification",
            Description = "Fully agentic CER workflow executed as a 3-node Microsoft Agent Framework graph: extracts candidate terms from source text, classifies them into one taxonomy category, and records the classification.",
            WorkflowType = "Agentic",
            Version = "v1",
            Collect = new PlaybookStepInput
            {
                Name = "Extract taxonomy terms",
                Description = "Asks the model to extract the key subject-matter terms from the Collect-stage input text.",
                ActionFormat = PlaybookActionFormat.Prompt,
                ActionDefinition = "Extract the key subject-matter terms from the text below. Respond with JSON only, matching exactly this shape: {\"terms\": [\"term1\", \"term2\"]}. Do not include any other text.\n\nText:\n{input}"
            },
            Evaluate = new PlaybookStepInput
            {
                Name = "Classify into taxonomy category",
                Description = "Asks the model to assign the extracted terms to exactly one category from the discrete rubric (taxonomy.categories v1): Finance, Healthcare, Technology, Legal, Other.",
                ActionFormat = PlaybookActionFormat.Prompt,
                ActionDefinition = "Classify the extracted terms into exactly one of the categories in the discrete scale below.\n- Finance: Budgeting, accounting, investments, and financial reporting.\n- Healthcare: Clinical care, medical records, and health regulations.\n- Technology: Software, infrastructure, and engineering practices.\n- Legal: Contracts, compliance, and regulatory matters.\n- Other: Content that does not clearly match another category.\n\nExtracted terms: {terms}\n\nRespond with JSON only: {\"category\": \"<one of the category names above>\", \"confidence\": <0 to 1>, \"rationale\": \"<short reason>\"}"
            },
            Record = new PlaybookStepInput
            {
                Name = "Record classification",
                Description = "Projects the finding into a human-readable summary.",
                ActionFormat = PlaybookActionFormat.Template,
                ActionDefinition = "Classified as {Category} (confidence {Confidence})."
            }
        };

        yield return new CreatePlaybookCommand
        {
            Key = "essay",
            Name = "Essay Rubric Evaluation",
            Description = "Hybrid CER workflow using the same 3-node Microsoft Agent Framework graph shape as Taxonomy: deterministic Collect and Record surround a single agentic Evaluate stage, scoring an essay against a weighted rubric.",
            WorkflowType = "Hybrid (Deterministic Collect/Record, Agentic Evaluate)",
            Version = "v1",
            Collect = new PlaybookStepInput
            {
                Name = "Collect essay text",
                Description = "Passes the Collect-stage input string (the essay text) through unchanged, timestamped as evidence. No LLM involvement.",
                ActionFormat = PlaybookActionFormat.Template,
                ActionDefinition = "EssayEvidence(EssayText: {input}, CollectedUtc: now)"
            },
            Evaluate = new PlaybookStepInput
            {
                Name = "Score against rubric",
                Description = "Asks the model to score the essay against each weighted criterion of rubric essay.rubric v1, then deterministically computes the weighted overall score.",
                ActionFormat = PlaybookActionFormat.Prompt,
                ActionDefinition = "Score the essay against each rubric criterion on a 0 to 1 scale.\n- Thesis clarity (weight 0.25): The essay states a clear, arguable thesis.\n- Evidence and support (weight 0.35): Claims are backed by relevant evidence or reasoning.\n- Organization (weight 0.25): Ideas flow logically from introduction to conclusion.\n- Grammar and mechanics (weight 0.15): The writing is free of significant grammar or spelling errors.\n\nEssay:\n{EssayText}\n\nRespond with JSON only: {\"scores\": [{\"criterion\": \"<criterion name>\", \"score\": <0 to 1>, \"reason\": \"<short reason>\"}]}. Include exactly one entry per criterion listed above."
            },
            Record = new PlaybookStepInput
            {
                Name = "Record scorecard",
                Description = "Projects the per-criterion scores and weighted overall score into a letter-graded scorecard summary.",
                ActionFormat = PlaybookActionFormat.Template,
                ActionDefinition = "Overall score {OverallScore} ({Grade}). Breakdown: {Breakdown}."
            }
        };
    }
}
