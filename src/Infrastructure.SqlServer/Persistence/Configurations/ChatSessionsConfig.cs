using Goodtocode.AgentFramework.Core.Domain.Chat;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence.Configurations;

public class ChatSessionsConfig : IEntityTypeConfiguration<ChatSessionEntity>
{
    public void Configure(EntityTypeBuilder<ChatSessionEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("ChatSessions");

        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Ignore(x => x.PartitionKey);
        builder.HasIndex(x => x.Timestamp).IsClustered().IsUnique();

        builder
            .HasMany(cs => cs.Messages)
            .WithOne(cm => cm.ChatSession)
            .HasForeignKey(cm => cm.ChatSessionId);
    }
}