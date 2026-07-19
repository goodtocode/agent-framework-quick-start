using System.Reflection;
using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Domain.Actor;
using Goodtocode.AgentFramework.Core.Domain.Chat;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence;

public class AgentFrameworkContext : DbContext, IAgentFrameworkContext
{
    private readonly IRlsContext? CurrentRlsContext;

    public DbSet<ChatMessageEntity> ChatMessages => Set<ChatMessageEntity>();
    public DbSet<ChatSessionEntity> ChatSessions => Set<ChatSessionEntity>();
    public DbSet<ActorEntity> Actors => Set<ActorEntity>();

    protected AgentFrameworkContext() { }

    public AgentFrameworkContext(
        DbContextOptions<AgentFrameworkContext> options,
        IRlsContext currentRlsContext) : base(options)
    {
        CurrentRlsContext = currentRlsContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema("Chat");

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
                    auditable.MarkModified(DateTime.UtcNow);
                }
                else if (entry.State == EntityState.Deleted)
                {
                    auditable.MarkDeleted(DateTime.UtcNow);
                    entry.State = EntityState.Modified;
                }
            }

            if (entry.Entity is ISecurable securable && CurrentRlsContext is not null)
            {
                if (entry.State == EntityState.Added)
                {
                    if (securable.OwnerId == Guid.Empty)
                        securable.ChangeOwner(CurrentRlsContext.OwnerId);
                    if (securable.TenantId == Guid.Empty)
                        securable.ChangeTenant(CurrentRlsContext.TenantId);
                }
            }
        }
    }
}
