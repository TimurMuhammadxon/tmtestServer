using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Content;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class RoadSignCategoryConfiguration : IEntityTypeConfiguration<RoadSignCategory>
{
    public void Configure(EntityTypeBuilder<RoadSignCategory> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.OrderIndex).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.HasMany(c => c.Translations)
            .WithOne()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Signs)
            .WithOne()
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Metadata.FindNavigation(nameof(RoadSignCategory.Translations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(RoadSignCategory.Signs))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
