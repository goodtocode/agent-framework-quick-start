using Goodtocode.AgentFramework.Core.Domain.Playbooks;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence.Configurations;

public class PlaybookExecutionsConfig : IEntityTypeConfiguration<PlaybookExecutionEntity>
{
    public void Configure(EntityTypeBuilder<PlaybookExecutionEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("PlaybookExecutions");

        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Ignore(x => x.PartitionKey);
        builder.HasIndex(x => x.Timestamp).IsClustered().IsUnique();

        builder.Property(x => x.PlaybookKey)
            .HasColumnType(ColumnTypes.Nvarchar200)
            .IsRequired();

        builder.Property(x => x.PlaybookVersion)
            .HasColumnType(ColumnTypes.Nvarchar100)
            .IsRequired();

        builder.Property(x => x.WorkflowType)
            .HasColumnType(ColumnTypes.Nvarchar100)
            .IsRequired();

        builder.Property(x => x.ReplayMode)
            .HasColumnType(ColumnTypes.Nvarchar100)
            .IsRequired();

        builder.Property(x => x.SourceExecutionId)
            .HasColumnType(ColumnTypes.Nvarchar200);

        builder.Property(x => x.CollectInput)
            .HasColumnType(ColumnTypes.NvarcharMax)
            .IsRequired();

        builder.Property(x => x.CollectOutput)
            .HasColumnType(ColumnTypes.NvarcharMax)
            .IsRequired();

        builder.Property(x => x.EvaluateOutput)
            .HasColumnType(ColumnTypes.NvarcharMax)
            .IsRequired();

        builder.Property(x => x.RecordOutput)
            .HasColumnType(ColumnTypes.NvarcharMax)
            .IsRequired();

        builder.Property(x => x.EvidenceJson)
            .HasColumnType(ColumnTypes.NvarcharMax);

        builder.Property(x => x.FindingJson)
            .HasColumnType(ColumnTypes.NvarcharMax);

        builder.Property(x => x.OwnerId)
            .HasColumnType(ColumnTypes.Uniqueidentifier)
            .IsRequired();

        builder.Property(x => x.TenantId)
            .HasColumnType(ColumnTypes.Uniqueidentifier)
            .IsRequired();

        builder
            .HasOne(execution => execution.Playbook)
            .WithMany()
            .HasForeignKey(execution => execution.PlaybookId);

        builder.HasIndex(x => new { x.TenantId, x.OwnerId, x.PlaybookKey });
    }
}
