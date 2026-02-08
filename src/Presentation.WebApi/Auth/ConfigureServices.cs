using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace Goodtocode.AgentFramework.Presentation.WebApi.Auth;

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
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(
                    jwtOptions =>
                    {
                        configuration.GetSection("EntraExternalId").Bind(jwtOptions);
                        //jwtOptions.TokenValidationParameters.RoleClaimType = TokenRoleClaimTypes.Roles;
                    },
                    identityOptions =>
                    {
                        configuration.GetSection("EntraExternalId").Bind(identityOptions);
                    });

        services.AddScoped<IClaimsReader, HttpClaimsReader>();
        services.AddScoped<ICurrentUserContext, ClaimsCurrentUserContext>();
        services.AddScoped(typeof(IPipelineBehavior<>), typeof(UserContextBehavior<>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UserContextBehavior<,>));

        return services;
    }
}