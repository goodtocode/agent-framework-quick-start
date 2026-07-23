using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace Goodtocode.AgentFramework.Presentation.Api.Identity.Auth;

/// <summary>
/// Presentation Layer WebApi Authentication Configuration
/// </summary>
public static class ConfigureServices
{
    private struct TokenRoleClaimTypes
    {
        public const string Roles = "roles";
        public const string Groups = "groups"; // I.e. EID Security Groups
    }

    /// <summary>
    /// Configures authentication with Microsoft Identity Platform and registers user context services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddAuthenticationWithRoles(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EntraExternalIdApiOptions>()
            .Bind(configuration.GetSection(EntraExternalIdApiOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => Uri.TryCreate(options.Instance, UriKind.Absolute, out _),
                "Configuration value 'EntraExternalId:Instance' must be an absolute URL.")
            .ValidateOnStart();

        var entraExternalIdOptions = configuration
            .GetSection(EntraExternalIdApiOptions.SectionName)
            .Get<EntraExternalIdApiOptions>() ?? throw new InvalidOperationException($"Missing '{EntraExternalIdApiOptions.SectionName}' configuration section.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(
                    jwtOptions =>
                    {
                        configuration.GetSection(EntraExternalIdApiOptions.SectionName).Bind(jwtOptions);
                        //jwtOptions.TokenValidationParameters.RoleClaimType = TokenRoleClaimTypes.Roles;
                    },
                    identityOptions =>
                    {
                        identityOptions.Instance = entraExternalIdOptions.Instance;
                        identityOptions.TenantId = entraExternalIdOptions.TenantId;
                        identityOptions.ClientId = entraExternalIdOptions.ClientId;
                    });

        services.AddScoped<IClaimsReader, HttpClaimsReader>();
        services.AddScoped<IRlsContext, ClaimsRlsContext>();
        services.AddScoped(typeof(IPipelineBehavior<>), typeof(InjectUserContextBehavior<>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(InjectUserContextBehavior<,>));

        return services;
    }
}
