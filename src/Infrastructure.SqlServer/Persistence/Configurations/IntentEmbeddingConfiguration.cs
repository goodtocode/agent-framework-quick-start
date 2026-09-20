using Microsoft.EntityFrameworkCore.ChangeTracking;
using Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence.Entities;
using System.Text.Json;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence.Configurations;

/// <summary>
/// EF Core entity configuration for <see cref="IntentEmbeddingEntity"/>.
/// Defines table mapping, constraints, indexes, and value converters.
/// </summary>
public sealed class IntentEmbeddingConfiguration : IEntityTypeConfiguration<IntentEmbeddingEntity>
{
    private static readonly ValueComparer<float[]> VectorComparer = new(
        (left, right) => left != null && right != null && left.SequenceEqual(right),
        vector => vector.Aggregate(0, (hash, value) => HashCode.Combine(hash, value)),
        vector => vector.ToArray());

    private static string SerializeVector(float[] vector)
    {
        return JsonSerializer.Serialize(vector);
    }

    private static float[] DeserializeVector(string json)
    {
        return JsonSerializer.Deserialize<float[]>(json) ?? Array.Empty<float>();
    }

    /// <summary>
    /// Configures the IntentEmbeddingEntity for EF Core.
    /// Table: [Chat].[IntentEmbeddings]
    /// </summary>
    public void Configure(EntityTypeBuilder<IntentEmbeddingEntity> builder)
    {
        // Table & Schema
        builder.ToTable("IntentEmbeddings", "Chat");

        // Primary Key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.IntentName)
            .IsRequired()
            .HasMaxLength(256)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.Property(e => e.Source)
            .IsRequired()
            .HasDefaultValue(0);  // Example = 0

        builder.Property(e => e.SourceText)
            .IsRequired()
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(e => e.Vector)
            .IsRequired()
            .Metadata.SetValueComparer(VectorComparer);

        builder.Property(e => e.Vector)
            .HasConversion(
                // To database: serialize float[] as JSON string
                v => SerializeVector(v),
                // From database: deserialize JSON string to float[]
                v => DeserializeVector(v))
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(e => e.Weight)
            .IsRequired()
            .HasDefaultValue(1.0f)
            .HasColumnType("REAL");

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired()
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAtUtc)
            .IsRequired()
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(e => e.IntentName)
            .HasDatabaseName("IX_IntentEmbeddings_IntentName");

        builder.HasIndex(e => new { e.IntentName, e.Source })
            .HasDatabaseName("IX_IntentEmbeddings_IntentName_Source");

        // Column mappings
        builder.Property(e => e.Id).HasColumnName("Id");
        builder.Property(e => e.IntentName).HasColumnName("IntentName");
        builder.Property(e => e.Source).HasColumnName("Source");
        builder.Property(e => e.SourceText).HasColumnName("SourceText");
        builder.Property(e => e.Vector).HasColumnName("Vector");
        builder.Property(e => e.Weight).HasColumnName("Weight");
        builder.Property(e => e.CreatedAtUtc).HasColumnName("CreatedAtUtc");
        builder.Property(e => e.UpdatedAtUtc).HasColumnName("UpdatedAtUtc");
    }
}
