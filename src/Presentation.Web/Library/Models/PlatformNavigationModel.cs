using Goodtocode.AgentFramework.Presentation.Web.Library.HorizontalMenu.Models;

namespace Goodtocode.AgentFramework.Presentation.Web.Library.Models;

public sealed class PlatformNavigationModel
{
    public required string Prefix { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public bool Center { get; init; }
    public required IReadOnlyList<HorizontalMenuItem> Items { get; init; }

    public static PlatformNavigationModel AgentFramework { get; } = new()
    {
        Prefix = "agent-framework",
        Title = "Navigation",
        Description = "Manage platform-level assets, execution, diagnostics, and configuration.",
        Center = true,
        Items =
        [
            new("Home", "/"),
            new("Chat", "/Chat")
        ]
    };

    public static IReadOnlyList<PlatformNavigationModel> All { get; } =
    [
        AgentFramework
    ];
}
