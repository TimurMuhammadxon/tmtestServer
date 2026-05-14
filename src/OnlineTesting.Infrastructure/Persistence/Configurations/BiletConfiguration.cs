using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class BiletConfiguration : IEntityTypeConfiguration<Bilet>
{
    public void Configure(EntityTypeBuilder<Bilet> builder)
    {
        builder.ToTable("bilets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Number).IsRequired();
        builder.Property(b => b.IsDemo).IsRequired();
        builder.Property(b => b.IsActive).IsRequired();
        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt).IsRequired();

        builder.HasIndex(b => b.Number)
            .IsUnique()
            .HasDatabaseName("ux_bilets_number");

        builder.HasIndex(b => b.IsDemo)
            .IsUnique()
            .HasFilter("is_demo = true")
            .HasDatabaseName("ux_bilets_demo");

        builder.HasMany(b => b.BiletQuestions)
            .WithOne(bq => bq.Bilet!)
            .HasForeignKey(bq => bq.BiletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Bilet.BiletQuestions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}