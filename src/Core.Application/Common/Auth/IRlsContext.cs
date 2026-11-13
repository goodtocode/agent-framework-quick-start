namespace Goodtocode.AgentStart.Core.Application.Common.Auth;

public interface IRlsContext
{
    Guid OwnerId { get; }
    Guid TenantId { get; }
}