using Goodtocode.AgentFramework.Core.Domain.Chats;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence.Configurations;

public sealed class ChatRequestIdempotencyConfig : IEntityTypeConfiguration<ChatRequestIdempotencyEntity>
{
    public void Configure(EntityTypeBuilder<ChatRequestIdempotencyEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("ChatRequestIdempotency");

        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Ignore(x => x.PartitionKey);
        builder.HasIndex(x => x.Timestamp).IsClustered().IsUnique();

        builder.Property(x => x.Operation)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IdempotencyKey)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.RequestHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.OwnerId, x.Operation, x.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("IX_ChatRequestIdempotency_TenantOwnerOperationKey");
    }
}
