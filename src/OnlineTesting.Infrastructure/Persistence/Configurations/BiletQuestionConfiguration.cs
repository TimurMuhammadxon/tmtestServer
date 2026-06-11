using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class BiletQuestionConfiguration : IEntityTypeConfiguration<BiletQuestion>
{
    public void Configure(EntityTypeBuilder<BiletQuestion> builder)
    {
        builder.ToTable("bilet_questions");

        builder.HasKey(bq => new { bq.BiletId, bq.QuestionId });

        builder.Property(bq => bq.OrderIndex).IsRequired();

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_bilet_questions_order_range",
            "order_index >= 1"));

        builder.HasIndex(bq => new { bq.BiletId, bq.OrderIndex })
            .IsUnique()
            .HasDatabaseName("ux_bilet_questions_order");

        builder.HasIndex(bq => bq.QuestionId)
            .IsUnique()
            .HasDatabaseName("ux_bilet_questions_question");

        builder.HasOne(bq => bq.Question)
            .WithMany()
            .HasForeignKey(bq => bq.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}