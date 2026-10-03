using Goodtocode.AgentFramework.Core.Domain.Playbooks;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence.Configurations;

public class PlaybooksConfig : IEntityTypeConfiguration<PlaybookEntity>
{
    public void Configure(EntityTypeBuilder<PlaybookEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Playbooks");

        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Ignore(x => x.PartitionKey);
        builder.HasIndex(x => x.Timestamp).IsClustered().IsUnique();

        builder.Property(x => x.Key)
            .HasColumnType(ColumnTypes.Nvarchar200)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType(ColumnTypes.Nvarchar200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnType(ColumnTypes.Nvarchar1000)
            .IsRequired();

        builder.Property(x => x.WorkflowType)
            .HasColumnType(ColumnTypes.Nvarchar100)
            .IsRequired();

        builder.Property(x => x.Version)
            .HasColumnType(ColumnTypes.Nvarchar100)
            .IsRequired();

        builder.HasIndex(x => x.Key).IsUnique();

        builder
            .HasMany(playbook => playbook.Steps)
            .WithOne(step => step.Playbook)
            .HasForeignKey(step => step.PlaybookId);
    }
}
