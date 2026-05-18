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
        builder.Property(a => a.Flow).HasColumnName("flow_type").IsRequired();
        builder.Property(a => a.Status).IsRequired();
        builder.Property(a => a.StartedAt).IsRequired();
        builder.Property(a => a.FinishedAt);
        builder.Property(a => a.CorrectCount);
        // intentionally no FK — attempt history must survive bilet/link deletion
        builder.Property(a => a.BiletId);
        builder.Property(a => a.TestLinkId);

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
