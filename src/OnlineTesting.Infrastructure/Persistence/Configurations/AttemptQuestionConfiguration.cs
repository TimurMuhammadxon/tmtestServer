using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class AttemptQuestionConfiguration : IEntityTypeConfiguration<AttemptQuestion>
{
    public void Configure(EntityTypeBuilder<AttemptQuestion> builder)
    {
        builder.ToTable("attempt_questions");

        builder.HasKey(aq => new { aq.AttemptId, aq.QuestionId });

        builder.Property(aq => aq.OrderIndex).IsRequired();
        builder.Property(aq => aq.ChosenAnswerId);
        builder.Property(aq => aq.IsCorrect);
        builder.Property(aq => aq.AnsweredAt);

        builder.HasOne(aq => aq.Question)
            .WithMany()
            .HasForeignKey(aq => aq.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
