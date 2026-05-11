using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class AnswerTranslationConfiguration : IEntityTypeConfiguration<AnswerTranslation>
{
    public void Configure(EntityTypeBuilder<AnswerTranslation> builder)
    {
        builder.HasKey(t => new { t.AnswerId, t.LanguageCode });

        builder.Property(t => t.LanguageCode).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Text).IsRequired().HasMaxLength(500);

        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(t => t.LanguageCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}