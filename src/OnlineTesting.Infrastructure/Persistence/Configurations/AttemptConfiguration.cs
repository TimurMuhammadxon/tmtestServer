using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class AttemptConfiguration : IEntityTypeConfiguration<Attempt>
{
    public void Configure(EntityTypeBuilder<Attempt> builder)
    {
        builder.ToTable("attempts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.FlowType).IsRequired();
        builder.Property(a => a.Status).IsRequired();
        builder.Property(a => a.StartedAt).IsRequired();
        builder.Property(a => a.FinishedAt);
        builder.Property(a => a.CorrectCount);
        builder.Property(a => a.BiletId);

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("ix_attempts_user_id");

        builder.HasMany(a => a.Questions)
            .WithOne(aq => aq.Attempt!)
            .HasForeignKey(aq => aq.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Attempt.Questions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
