using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Teacher;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.TeacherId).HasColumnName("teacher_id");
        builder.Property(g => g.Name).HasColumnName("name").HasMaxLength(200);
        builder.Property(g => g.InviteCode).HasColumnName("invite_code").HasMaxLength(10);
        builder.Property(g => g.IsActive).HasColumnName("is_active");
        builder.Property(g => g.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(g => g.InviteCode)
            .IsUnique()
            .HasDatabaseName("ux_groups_invite_code");

        builder.HasIndex(g => g.TeacherId)
            .HasDatabaseName("ix_groups_teacher_id");

        builder.HasMany(g => g.Members)
            .WithOne()
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
