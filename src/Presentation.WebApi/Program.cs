using Azure.Monitor.OpenTelemetry.AspNetCore;
using Goodtocode.AgentFramework.Core.Application;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework;
using Goodtocode.AgentFramework.Infrastructure.SqlServer;
using Goodtocode.AgentFramework.Presentation.WebApi;
using Goodtocode.AgentFramework.Presentation.WebApi.Auth;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

var builder = WebApplication.CreateBuilder(args);

builder.AddLocalEnvironment();

builder.Services.AddAuthenticationWithRoles(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddDbContextServices(builder.Configuration);
builder.Services.AddAgentFrameworkOpenAIServices(builder.Configuration);
builder.Services.AddWebUIServices();
builder.Services.AddHealthChecks();

BuildApiVerAndApiExplorer(builder);

builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsLocal())
{
    app.UseSwagger();
    UseSwaggerUiConfigs();
}

app.UseRouting();
app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseCors("AllowOrigin");
app.MapControllers();
app.Run();

void UseSwaggerUiConfigs()
{
    var providers = app.Services.GetService<IApiVersionDescriptionProvider>();

    app.UseSwaggerUI(options =>
    {
        if (providers == null) return;
        foreach (var description in providers.ApiVersionDescriptions)
            options.SwaggerEndpoint($"../swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
    });
}

void BuildApiVerAndApiExplorer(WebApplicationBuilder webApplicationBuilder)
{
    webApplicationBuilder.Services.AddApiVersioning(setup =>
    {
        setup.DefaultApiVersion = new ApiVersion(1, 0);
        setup.AssumeDefaultVersionWhenUnspecified = true;
        setup.ReportApiVersions = true;
    })
    .AddApiExplorer(setup =>
    {
        setup.GroupNameFormat = "'v'VVV";
        setup.SubstituteApiVersionInUrl = true;
    });
}
