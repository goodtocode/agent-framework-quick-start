using Goodtocode.AgentFramework.Core.Domain.Playbooks;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence.Configurations;

public class PlaybookMaterializationsConfig : IEntityTypeConfiguration<PlaybookMaterializationEntity>
{
    public void Configure(EntityTypeBuilder<PlaybookMaterializationEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("PlaybookMaterializations");

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

        builder.Property(x => x.SummaryText)
            .HasColumnType(ColumnTypes.Nvarchar1000)
            .IsRequired();

        builder.Property(x => x.PayloadSnapshot)
            .HasColumnType(ColumnTypes.NvarcharMax)
            .IsRequired();

        builder.Property(x => x.OwnerId)
            .HasColumnType(ColumnTypes.Uniqueidentifier)
            .IsRequired();

        builder.Property(x => x.TenantId)
            .HasColumnType(ColumnTypes.Uniqueidentifier)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.OwnerId, x.PlaybookKey });
    }
}
