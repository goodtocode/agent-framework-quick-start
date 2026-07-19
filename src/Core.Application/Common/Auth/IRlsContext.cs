namespace Goodtocode.AgentFramework.Core.Application.Common.Auth;

public interface IRlsContext
{
    Guid OwnerId { get; }
    Guid TenantId { get; }
}