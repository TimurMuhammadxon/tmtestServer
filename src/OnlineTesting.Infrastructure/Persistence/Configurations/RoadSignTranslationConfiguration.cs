using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Content;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class RoadSignTranslationConfiguration : IEntityTypeConfiguration<RoadSignTranslation>
{
    public void Configure(EntityTypeBuilder<RoadSignTranslation> builder)
    {
        builder.HasKey(t => new { t.SignId, t.LanguageCode });

        builder.Property(t => t.LanguageCode).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(300);
        builder.Property(t => t.Description).HasMaxLength(2000);

        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(t => t.LanguageCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
