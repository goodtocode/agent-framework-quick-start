namespace Goodtocode.AgentStart.Presentation.Web.Options;

/// <summary>
/// Application Insights (Azure Monitor) settings. The section is required; the connection string may be empty.
/// </summary>
public sealed class ApplicationInsightsOptions
{
    /// <summary>
    /// Configuration section name used to bind Application Insights settings.
    /// </summary>
    public const string SectionName = "ApplicationInsights";

    /// <summary>
    /// Standard Azure environment variable, set on App Service by the infrastructure deployment.
    /// </summary>
    public const string ConnectionStringEnvironmentVariable = "APPLICATIONINSIGHTS_CONNECTION_STRING";

    /// <summary>
    /// Application Insights connection string. Empty disables Azure Monitor telemetry, for example on a new local install before the infrastructure exists.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether a connection string is available, so Azure Monitor telemetry is on.
    /// </summary>
    public bool IsEnabled => !string.IsNullOrWhiteSpace(ConnectionString);

    /// <summary>
    /// Uses the standard environment variable when no connection string is configured.
    /// </summary>
    public void ApplyEnvironmentFallback()
    {
        if (!IsEnabled)
            ConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable) ?? string.Empty;
    }
}
