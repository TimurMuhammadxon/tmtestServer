using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class TopicTranslationConfiguration : IEntityTypeConfiguration<TopicTranslation>
{
    public void Configure(EntityTypeBuilder<TopicTranslation> builder)
    {
        builder.HasKey(t => new { t.TopicId, t.LanguageCode });

        builder.Property(t => t.LanguageCode).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);

        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(t => t.LanguageCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}