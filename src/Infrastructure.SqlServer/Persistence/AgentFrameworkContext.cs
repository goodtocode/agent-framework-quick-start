using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Domain.Actor;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;
using Goodtocode.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence;

public class AgentFrameworkContext : DbContext, IAgentFrameworkContext
{
    private readonly ICurrentUserContext? _currentUserContext;

    public DbSet<ChatMessageEntity> ChatMessages => Set<ChatMessageEntity>();
    public DbSet<ChatSessionEntity> ChatSessions => Set<ChatSessionEntity>();
    public DbSet<ActorEntity> Actors => Set<ActorEntity>();

    protected AgentFrameworkContext() { }

    public AgentFrameworkContext(
        DbContextOptions<AgentFrameworkContext> options,
        ICurrentUserContext currentUserContext) : base(options)
    {
        _currentUserContext = currentUserContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(),
            x => x.Namespace == $"{GetType().Namespace}.Configurations");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditAndSecurityFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditAndSecurityFields()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditable auditable)
            {
                if (entry.State == EntityState.Modified)
                {
                    auditable.MarkModified();
                    auditable.MarkDeleted();
                }
                else if (entry.State == EntityState.Deleted)
                {
                    auditable.MarkDeleted();
                    entry.State = EntityState.Modified;
                }
            }

            if (entry.Entity is ISecurable securable && _currentUserContext is not null)
            {
                if (entry.State == EntityState.Added)
                {
                    if (securable.OwnerId == Guid.Empty)
                        securable.ChangeOwner(_currentUserContext.OwnerId);
                    if (securable.TenantId == Guid.Empty)
                        securable.ChangeTenant(_currentUserContext.TenantId);
                }
            }
        }
    }

    private static bool IsDomainEntity(object entity)
    {
        var type = entity.GetType();
        while (type != null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DomainEntity<>))
                return true;
            type = type.BaseType;
        }
        return false;
    }
}
