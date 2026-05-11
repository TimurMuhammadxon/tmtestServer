using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class QuestionTranslationConfiguration : IEntityTypeConfiguration<QuestionTranslation>
{
    public void Configure(EntityTypeBuilder<QuestionTranslation> builder)
    {
        builder.HasKey(t => new { t.QuestionId, t.LanguageCode });

        builder.Property(t => t.LanguageCode).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Text).IsRequired().HasMaxLength(2000);
        builder.Property(t => t.Explanation).HasMaxLength(4000);

        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(t => t.LanguageCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}