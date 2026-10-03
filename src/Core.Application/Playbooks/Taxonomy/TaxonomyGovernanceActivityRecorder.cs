using Goodtocode.AgentFramework.Core.Application.Common.Auth;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Governance;
using Goodtocode.Agents.Governance.Application;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;

/// <summary>
/// Projects the Taxonomy Extraction and Classification playbook's CER outputs into governance
/// summaries, consumed by <see cref="SharedPlaybookGovernanceActivityRecorder{TEvidence,TFinding,TMaterialization}"/>.
/// </summary>
public sealed class TaxonomyStageSummarySelector : IPlaybookStageSummarySelector<TaxonomyEvidence, TaxonomyFinding, TaxonomyMaterialization>
{
    public string SummarizeEvidence(TaxonomyEvidence evidence) =>
        $"SourceText: {evidence.SourceText}, ExtractedTerms: [{string.Join(", ", evidence.ExtractedTerms)}]";

    public string SummarizeFinding(TaxonomyFinding finding) =>
        $"Category: {finding.Category}, Confidence: {finding.Confidence:0.##}, Rationale: {finding.Rationale}";

    public string SummarizeMaterialization(TaxonomyMaterialization materialization) => materialization.Summary;
}

/// <summary>
/// Taxonomy Extraction and Classification example playbook's governance activity recorder: a thin
/// specialization of <see cref="SharedPlaybookGovernanceActivityRecorder{TEvidence,TFinding,TMaterialization}"/>.
/// All three stages resolve to agentic tools, so every governance record's
/// <c>Repeatability.DeterministicReplaySupported</c> is false.
/// </summary>
public sealed class TaxonomyGovernanceActivityRecorder(IRlsContext userContext, IRepeatabilityHashStrategy hashStrategy)
    : SharedPlaybookGovernanceActivityRecorder<TaxonomyEvidence, TaxonomyFinding, TaxonomyMaterialization>(
        userContext,
        hashStrategy,
        new TaxonomyStageSummarySelector());
