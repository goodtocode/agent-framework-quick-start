using System.Text.Json;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks;

/// <summary>
/// Shared helper for agentic Playbook stage tools: validates and deserializes a model's raw text
/// response into a typed contract. No untyped JSON/dynamic value is ever allowed to cross a CER
/// stage boundary - a response that cannot be parsed into <typeparamref name="T"/> fails the stage
/// with a descriptive exception rather than propagating raw model output.
/// </summary>
internal static class AgentStructuredOutputParser
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static T ParseOrThrow<T>(string? responseText, string toolName)
    {
        if (string.IsNullOrWhiteSpace(responseText))
        {
            throw new InvalidOperationException($"{toolName} received an empty model response.");
        }

        var json = ExtractJson(responseText);

        try
        {
            var result = JsonSerializer.Deserialize<T>(json, Options);
            return result ?? throw new InvalidOperationException($"{toolName} received a model response that deserialized to null.");
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                $"{toolName} received a model response that could not be parsed as structured JSON output: '{responseText}'.",
                exception);
        }
    }

    private static string ExtractJson(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var firstNewline = trimmed.IndexOf('\n');
        if (firstNewline < 0)
        {
            return trimmed;
        }

        var withoutOpeningFence = trimmed[(firstNewline + 1)..];
        var closingFenceIndex = withoutOpeningFence.LastIndexOf("```", StringComparison.Ordinal);
        return closingFenceIndex >= 0 ? withoutOpeningFence[..closingFenceIndex].Trim() : withoutOpeningFence.Trim();
    }
}
