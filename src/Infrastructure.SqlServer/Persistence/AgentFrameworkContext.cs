using System.Reflection;
using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Domain.Actor;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;
using Goodtocode.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
        SetSecurityFields();
        SetAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetSecurityFields()
    {
        if (_currentUserContext is null ||
            _currentUserContext.OwnerId == Guid.Empty ||
            _currentUserContext.TenantId == Guid.Empty)
        {
            return;
        }

        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added && HasSecurityFields(e));

        foreach (var entry in entries)
        {
            SetIfEmpty(entry, "OwnerId", _currentUserContext.OwnerId);
            SetIfEmpty(entry, "TenantId", _currentUserContext.TenantId);
        }
    }

    private static bool HasSecurityFields(EntityEntry entry) =>
        entry.Metadata.FindProperty("OwnerId") != null &&
        entry.Metadata.FindProperty("TenantId") != null;

    private static void SetIfEmpty(EntityEntry entry, string propertyName, Guid value)
    {
        var property = entry.Property(propertyName);
        if (property.CurrentValue is Guid guid && guid == Guid.Empty)
        {
            property.CurrentValue = value;
        }
    }

    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => IsDomainEntity(e.Entity) &&
                       (e.State == EntityState.Modified || e.State == EntityState.Added || e.State == EntityState.Deleted));

        foreach (var entry in entries)
        {
            dynamic entity = entry.Entity;
            if (entry.State == EntityState.Added)
            {
                entity.SetCreatedOn(DateTime.UtcNow);
                entity.SetModifiedOn(null);
                entity.SetDeletedOn(null);
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.SetModifiedOn(DateTime.UtcNow);
                entity.SetDeletedOn(null);
            }
            else if (entry.State == EntityState.Deleted)
            {
                entity.SetDeletedOn(DateTime.UtcNow);
                entry.State = EntityState.Modified;
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
