namespace Goodtocode.AgentFramework.Core.Application.Abstractions;

public interface ICurrentUserContext
{
    Guid OwnerId { get; }
    Guid TenantId { get; }
}