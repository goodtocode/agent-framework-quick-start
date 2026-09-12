using Goodtocode.AgentFramework.Core.Domain.Common;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence.Configurations;

public sealed class RequestIdempotencyConfig : IEntityTypeConfiguration<RequestIdempotencyEntity>
{
    public void Configure(EntityTypeBuilder<RequestIdempotencyEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("RequestIdempotency");

        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Ignore(x => x.PartitionKey);
        builder.HasIndex(x => x.Timestamp).IsClustered().IsUnique();

        builder.Property(x => x.OperationKey)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.IdempotencyKey)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.RequestHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.ResponseType)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ResponsePayload)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.ResourceType)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.HasIndex(x => new { x.TenantId, x.OwnerId, x.OperationKey, x.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("IX_RequestIdempotency_TenantOwnerOperationKey");

        builder.HasIndex(x => new { x.TenantId, x.OwnerId, x.OperationKey, x.ScopeId, x.RequestHash, x.Timestamp })
            .HasDatabaseName("IX_RequestIdempotency_DuplicateWindowLookup");
    }
}
