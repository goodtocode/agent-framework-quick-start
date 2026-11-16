using System.Reflection;
using Goodtocode.AgentStart.Core.Application.Abstractions;
using Goodtocode.AgentStart.Core.Domain.Actors;
using Goodtocode.AgentStart.Core.Domain.Chats;
using Goodtocode.AgentStart.Core.Domain.Common;
using Goodtocode.AgentStart.Core.Domain.Governance;
using Goodtocode.AgentStart.Core.Domain.Playbooks;
using Goodtocode.AgentStart.Infrastructure.SqlServer.Persistence.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Goodtocode.AgentStart.Infrastructure.SqlServer.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IRlsContext? CurrentRlsContext;

    public DbSet<ChatMessageEntity> ChatMessages => Set<ChatMessageEntity>();
    public DbSet<ChatSessionEntity> ChatSessions => Set<ChatSessionEntity>();
    public DbSet<RequestIdempotencyEntity> RequestIdempotency => Set<RequestIdempotencyEntity>();
    public DbSet<ActorEntity> Actors => Set<ActorEntity>();
    public DbSet<ChatGovernanceEntity> ChatGovernance => Set<ChatGovernanceEntity>();
    public DbSet<IntentEmbeddingEntity> IntentEmbeddings => Set<IntentEmbeddingEntity>();
    public DbSet<PlaybookEntity> Playbooks => Set<PlaybookEntity>();
    public DbSet<PlaybookStepEntity> PlaybookSteps => Set<PlaybookStepEntity>();
    public DbSet<PlaybookExecutionEntity> PlaybookExecutions => Set<PlaybookExecutionEntity>();

    protected ApplicationDbContext() { }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
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
