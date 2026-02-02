using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Domain.Actor;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;
using Goodtocode.Domain.Entities;
using System.Reflection;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence;

public class AgentFrameworkContext : DbContext, IAgentFrameworkContext
{
    public DbSet<ChatMessageEntity> ChatMessages => Set<ChatMessageEntity>();
    public DbSet<ChatSessionEntity> ChatSessions => Set<ChatSessionEntity>();
    public DbSet<ActorEntity> Actors => Set<ActorEntity>();

    protected AgentFrameworkContext() { }

    public AgentFrameworkContext(DbContextOptions<AgentFrameworkContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(),
            x => x.Namespace == $"{GetType().Namespace}.Configurations");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return base.SaveChangesAsync(cancellationToken);
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
