using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Teacher;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class TestLinkConfiguration : IEntityTypeConfiguration<TestLink>
{
    public void Configure(EntityTypeBuilder<TestLink> builder)
    {
        builder.ToTable("test_links");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TeacherId).IsRequired();
        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Code).HasMaxLength(10).IsRequired();
        builder.Property(t => t.FlowType).HasColumnName("flow_type").IsRequired();
        builder.Property(t => t.BiletId);
        builder.Property(t => t.TopicIds).HasColumnType("uuid[]");
        builder.Property(t => t.QuestionCount);
        builder.Property(t => t.GroupId);
        builder.Property(t => t.MaxAttempts).IsRequired();
        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.IsActive).IsRequired();
        builder.Property(t => t.ShowExplanations).IsRequired().HasDefaultValue(false);
        builder.Property(t => t.CreatedAt).IsRequired();

        builder.HasIndex(t => t.Code)
            .HasDatabaseName("ux_test_links_code")
            .IsUnique();

        builder.HasIndex(t => t.TeacherId)
            .HasDatabaseName("ix_test_links_teacher_id");
    }
}
