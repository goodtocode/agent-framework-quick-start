using Goodtocode.AgentFramework.Core.Domain.Governance;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence.Configurations;

public class ChatGovernanceConfig : IEntityTypeConfiguration<ChatGovernanceEntity>
{
    public void Configure(EntityTypeBuilder<ChatGovernanceEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("ChatGovernance");
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Ignore(x => x.PartitionKey);
        builder.HasIndex(x => new { x.TenantId, x.OwnerId, x.ChatSessionId });
        builder.HasIndex(x => x.Timestamp).IsClustered().IsUnique();
        builder.Property(x => x.PolicyProfileVersion).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TraceId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PrincipalDisplay).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ModelRef).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ModelVersion).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PromptHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.InputHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.SystemInstruction).IsRequired();
        builder.Property(x => x.MetadataJson).IsRequired();
        builder.Property(x => x.EvidenceRefsJson).IsRequired();
        builder.Property(x => x.ToolRefsJson).IsRequired();
        builder.Property(x => x.PoliciesAppliedJson).IsRequired();
        builder.Property(x => x.JustificationRefsJson).IsRequired();
        builder.Property(x => x.ReasoningSummary).IsRequired();
    }
}