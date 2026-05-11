using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.QuestionId).IsRequired();
        builder.Property(a => a.OrderIndex).IsRequired();
        builder.Property(a => a.IsCorrect).IsRequired();

        builder.HasIndex(a => a.QuestionId);

        builder.HasIndex(a => new { a.QuestionId, a.IsCorrect })
            .HasFilter("is_correct = true")
            .IsUnique()
            .HasDatabaseName("ux_answers_question_correct");

        builder.HasMany(a => a.Translations)
            .WithOne()
            .HasForeignKey(t => t.AnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Answer.Translations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

    }
}