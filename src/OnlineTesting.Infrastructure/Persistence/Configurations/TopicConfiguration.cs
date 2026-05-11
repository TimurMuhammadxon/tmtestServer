using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.Code).IsUnique();

        builder.Property(t => t.OrderIndex).IsRequired();
        builder.Property(t => t.IsDemo).IsRequired();
        builder.Property(t => t.IsActive).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        builder.HasIndex(t => t.IsDemo)
            .HasFilter("is_demo = true")
            .IsUnique()
            .HasDatabaseName("ux_topics_demo");

        builder.HasMany(t => t.Translations)
            .WithOne()
            .HasForeignKey(tr => tr.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Topic.Translations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

    }
}