using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.TopicId).IsRequired();
        builder.Property(q => q.ImageKey).HasMaxLength(500);
        builder.Property(q => q.IsActive).IsRequired();
        builder.Property(q => q.CreatedAt).IsRequired();
        builder.Property(q => q.UpdatedAt).IsRequired();

        builder.HasIndex(q => q.TopicId);

        builder.HasOne<Topic>()
            .WithMany()
            .HasForeignKey(q => q.TopicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(q => q.Answers)
            .WithOne()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(q => q.Translations)
            .WithOne()
            .HasForeignKey(t => t.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Question.Answers))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata.FindNavigation(nameof(Question.Translations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

    }
}