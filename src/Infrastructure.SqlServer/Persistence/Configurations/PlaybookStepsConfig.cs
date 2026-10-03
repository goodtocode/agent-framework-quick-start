using Goodtocode.AgentFramework.Core.Domain.Playbooks;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence.Configurations;

public class PlaybookStepsConfig : IEntityTypeConfiguration<PlaybookStepEntity>
{
    public void Configure(EntityTypeBuilder<PlaybookStepEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("PlaybookSteps");

        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Ignore(x => x.PartitionKey);
        builder.HasIndex(x => x.Timestamp).IsClustered().IsUnique();

        builder.Property(x => x.StepType)
            .HasConversion<string>()
            .HasColumnType(ColumnTypes.Nvarchar100)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType(ColumnTypes.Nvarchar200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnType(ColumnTypes.Nvarchar1000)
            .IsRequired();

        builder.Property(x => x.ActionFormat)
            .HasConversion<string>()
            .HasColumnType(ColumnTypes.Nvarchar100)
            .IsRequired();

        builder.Property(x => x.ActionDefinition)
            .HasColumnType(ColumnTypes.NvarcharMax)
            .IsRequired();

        builder.HasIndex(x => new { x.PlaybookId, x.StepType }).IsUnique();
    }
}
