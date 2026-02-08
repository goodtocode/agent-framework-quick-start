using Goodtocode.AgentFramework.Core.Application.Abstractions;

namespace Goodtocode.AgentFramework.Presentation.WebApi.Auth;

/// <summary>
/// Provides the current user context by reading from HTTP authentication claims.
/// </summary>
/// <remarks>This class bridges the infrastructure layer (claims reading) with the application layer
/// (ICurrentUserContext) by exposing OwnerId and TenantId from the authenticated user's claims.</remarks>
public sealed class ClaimsCurrentUserContext : ICurrentUserContext
{
    private readonly IClaimsReader claimsReader;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClaimsCurrentUserContext"/> class.
    /// </summary>
    /// <param name="claimsReader">The claims reader for accessing HTTP authentication context.</param>
    public ClaimsCurrentUserContext(IClaimsReader claimsReader)
    {
        this.claimsReader = claimsReader;
    }

    /// <summary>
    /// Gets the owner identifier from the authenticated user's object identifier claim.
    /// </summary>
    public Guid OwnerId => claimsReader.ObjectId;

    /// <summary>
    /// Gets the tenant identifier from the authenticated user's tenant claim.
    /// </summary>
    public Guid TenantId => claimsReader.TenantId;
}