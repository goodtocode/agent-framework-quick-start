namespace Goodtocode.AgentFramework.Presentation.Web.Options;

public class ResilientHttpClientOptions
{
    public Uri? BaseAddress { get; set; }
    public string? ClientName { get; set; }
    public int MaxRetry { get; set; } = 5;

    /// <summary>
    /// Default upper bound for one request. LLM-backed calls commonly take 10+ seconds.
    /// </summary>
    public const int DefaultTimeoutSeconds = 180;

    /// <summary>
    /// Upper bound for a single request attempt, including the API's LLM fallback.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(DefaultTimeoutSeconds);
}