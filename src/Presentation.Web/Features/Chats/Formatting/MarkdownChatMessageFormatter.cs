using System.Text.RegularExpressions;
using Markdig;

namespace Goodtocode.AgentFramework.Presentation.Web.Features.Chats.Formatting;

public sealed partial class MarkdownChatMessageFormatter : IChatMessageFormatter
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .DisableHtml()
        .UseAdvancedExtensions()
        .Build();

    public string FormatAssistantMessageAsHtml(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var html = Markdown.ToHtml(content, Pipeline);
        return SanitizeLinks(html);
    }

    private static string SanitizeLinks(string html)
    {
        return HrefRegex().Replace(html, match =>
        {
            var href = match.Groups[1].Value;
            return IsSafeHref(href) ? match.Value : "href=\"#\"";
        });
    }

    private static bool IsSafeHref(string href)
    {
        if (string.IsNullOrWhiteSpace(href))
        {
            return false;
        }

        if (!Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out var uri))
        {
            return false;
        }

        if (!uri.IsAbsoluteUri)
        {
            var trimmed = href.TrimStart();
            return !trimmed.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
                && !trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase);
        }

        return uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            || uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            || uri.Scheme.Equals(Uri.UriSchemeMailto, StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex("href=\"([^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex HrefRegex();
}
