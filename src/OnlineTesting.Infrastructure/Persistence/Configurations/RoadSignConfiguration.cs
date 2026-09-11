using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Content;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class RoadSignConfiguration : IEntityTypeConfiguration<RoadSign>
{
    public void Configure(EntityTypeBuilder<RoadSign> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code).HasMaxLength(30);
        builder.Property(s => s.ImageKey).HasMaxLength(300);
        builder.Property(s => s.OrderIndex).IsRequired();
        builder.Property(s => s.IsActive).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => s.CategoryId);

        builder.HasMany(s => s.Translations)
            .WithOne()
            .HasForeignKey(t => t.SignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(RoadSign.Translations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
