using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Teacher;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class TeacherApplicationConfiguration : IEntityTypeConfiguration<TeacherApplication>
{
    public void Configure(EntityTypeBuilder<TeacherApplication> builder)
    {
        builder.ToTable("teacher_applications");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.UserId).HasColumnName("user_id");
        builder.Property(a => a.FullName).HasColumnName("full_name").HasMaxLength(200);
        builder.Property(a => a.PhoneNumber).HasColumnName("phone_number").HasMaxLength(30);
        builder.Property(a => a.TelegramUsername).HasColumnName("telegram_username").HasMaxLength(100);
        builder.Property(a => a.OrganizationName).HasColumnName("organization_name").HasMaxLength(200);
        builder.Property(a => a.ExperienceText).HasColumnName("experience_text");
        builder.Property(a => a.AdditionalNotes).HasColumnName("additional_notes");
        builder.Property(a => a.Status).HasColumnName("status");
        builder.Property(a => a.SubmittedAt).HasColumnName("submitted_at");
        builder.Property(a => a.ReviewedAt).HasColumnName("reviewed_at");
        builder.Property(a => a.ReviewedBy).HasColumnName("reviewed_by");
        builder.Property(a => a.RejectionReason).HasColumnName("rejection_reason");

        // one pending application per user at a time
        builder.HasIndex(a => a.UserId)
            .HasFilter("status = 0")
            .IsUnique()
            .HasDatabaseName("ux_teacher_applications_user_pending");

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("ix_teacher_applications_status");
    }
}
