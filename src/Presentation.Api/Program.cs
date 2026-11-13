using Goodtocode.AgentStart.Core.Application;
using Goodtocode.AgentStart.Infrastructure.AgentFramework;
using Goodtocode.AgentStart.Infrastructure.SqlServer;
using Goodtocode.AgentStart.Presentation.Api;
using Goodtocode.AgentStart.Presentation.Api.Common.Exceptions;
using Goodtocode.AgentStart.Presentation.Api.Endpoints;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

var builder = WebApplication.CreateBuilder(args);

builder.AddLocalEnvironment();

builder.Services.AddAuthenticationWithRoles(builder.Configuration);

builder.Services.AddDbContextServices(builder.Configuration)
                .AddApplicationServices();
builder.Services.AddAgentFrameworkOpenAIServices(builder.Configuration);
builder.Services.AddWebUIServices();
builder.Services.AddApiExceptionHandling();
builder.Services.AddHealthChecks();

BuildApiVerAndApiExplorer(builder);

builder.AddAzureMonitorTelemetry();

var app = builder.Build();

app.WarnIfTelemetryDisabled();

if (app.Environment.IsDevelopment() || app.Environment.IsLocal())
{
    app.UseSwagger();
    UseSwaggerUiConfigs();
}

app.UseExceptionHandler();
app.UseRouting();
app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowOrigin");
app.MapMinimalApis();
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
