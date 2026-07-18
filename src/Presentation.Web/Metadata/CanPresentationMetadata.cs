using Microsoft.FluentUI.AspNetCore.Components;
using Icons = Microsoft.FluentUI.AspNetCore.Components.Icons;

namespace Goodtocode.AgentFramework.Presentation.Web.Metadata;

public record CanPresentationInfo(
    CanScheme Scheme,
    string DisplayName,
    Icon Icon,
    string Description,
    string[] Capabilities,
    bool Enabled
);

public static class CanPresentationMetadata
{
    public static readonly List<CanPresentationInfo> All =
    [
        new(CanScheme.Curator, "Curator", new Icons.Regular.Size24.PeopleAdd(), "Manages catalogs, assets, resources, metadata, connectors.", ["Catalogs", "Resources", "Relationships"], true),
        new(CanScheme.Conductor, "Conductor", new Icons.Regular.Size24.DeviceEq(), "Orchestrates agents, personas, relationships, enrollment, triggers.", ["Personas", "Connectors", "Authentication", "Triggers"], true),
        new(CanScheme.Compliance, "Compliance", new Icons.Regular.Size24.ShieldCheckmark(), "Handles policies, controls, evaluations, evidence, attestations.", ["Policies", "Controls", "Evaluations", "Alerts", "Allow List"], false),
        new(CanScheme.Codex, "Codex", new Icons.Regular.Size24.BookOpen(), "Stores chronicles, stories, sections, blocks.", ["Chronicles", "Stories", "Sections", "Blocks"], true),
        new(CanScheme.Crafting, "Crafting", new Icons.Regular.Size24.Wand(), "Provides prompts, templates, compositions, tools.", ["Recipes", "Templates", "Compositions", "Tools"], false),
        new(CanScheme.Clans, "Clans", new Icons.Regular.Size24.PeopleCommunity(), "Manages members, roles, groups, entitlements.", ["Members", "Roles", "Groups", "Entitlements"], false),
    ];

    public static CanPresentationInfo? GetByScheme(CanScheme scheme) =>
        All.Find(c => c.Scheme == scheme);
}
