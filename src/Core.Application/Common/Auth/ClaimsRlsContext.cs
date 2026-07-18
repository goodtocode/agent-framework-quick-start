namespace Goodtocode.AgentFramework.Core.Application.Common.Auth;

/// <summary>
/// Provides the current user context by reading from HTTP authentication claims.
/// </summary>
/// <remarks>This class bridges the infrastructure layer (claims reading) with the application layer
/// (ICurrentUserContext) by exposing OwnerId and TenantId from the authenticated user's claims.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="ClaimsRlsContext"/> class.
/// </remarks>
/// <param name="claimsReader">The claims reader for accessing HTTP authentication context.</param>
public sealed class ClaimsRlsContext(IClaimsReader claimsReader) : IRlsContext
{
    private readonly IClaimsReader claimsReader = claimsReader;

    /// <summary>
    /// Gets the owner identifier from the authenticated user's object identifier claim.
    /// </summary>
    public Guid OwnerId => claimsReader.ObjectId;

    /// <summary>
    /// Gets the tenant identifier from the authenticated user's tenant claim.
    /// </summary>
    public Guid TenantId => claimsReader.TenantId;
}