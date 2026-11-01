using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Monitor.OpenTelemetry.Exporter;
using Goodtocode.AgentFramework.Presentation.Api.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Goodtocode.AgentFramework.Presentation.Api;

/// <summary>
/// Presentation Layer telemetry configuration
/// </summary>
public static class ConfigureServicesTelemetry
{
    /// <summary>
    /// Adds OpenTelemetry instrumentation, and exports to Azure Monitor when a connection string is available.
    /// The ApplicationInsights section is required, but its connection string may be empty.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddAzureMonitorTelemetry(this WebApplicationBuilder builder)
    {
        var section = builder.Configuration.GetRequiredSection(ApplicationInsightsOptions.SectionName);

        builder.Services.AddOptions<ApplicationInsightsOptions>()
            .Bind(section)
            .PostConfigure(options => options.ApplyEnvironmentFallback());

        var settings = section.Get<ApplicationInsightsOptions>() ?? new ApplicationInsightsOptions();
        settings.ApplyEnvironmentFallback();

        var telemetry = builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation());

        if (settings.IsEnabled)
        {
            telemetry.UseAzureMonitor(options => options.ConnectionString = settings.ConnectionString);
            builder.Logging.AddOpenTelemetry(options =>
                options.AddAzureMonitorLogExporter(exporter => exporter.ConnectionString = settings.ConnectionString));
        }

        return builder;
    }

    /// <summary>
    /// Logs a warning when Azure Monitor telemetry is disabled because no connection string is available.
    /// </summary>
    /// <param name="app"></param>
    public static void WarnIfTelemetryDisabled(this WebApplication app)
    {
        if (!app.Services.GetRequiredService<IOptions<ApplicationInsightsOptions>>().Value.IsEnabled)
            app.Logger.LogWarning("ApplicationInsights connection string is not set; Azure Monitor telemetry is disabled.");
    }
}
