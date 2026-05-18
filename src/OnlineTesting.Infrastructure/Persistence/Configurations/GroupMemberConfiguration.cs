using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Teacher;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.ToTable("group_members");
        builder.HasKey(m => new { m.GroupId, m.UserId });
        builder.Property(m => m.GroupId).HasColumnName("group_id");
        builder.Property(m => m.UserId).HasColumnName("user_id");
        builder.Property(m => m.JoinedAt).HasColumnName("joined_at");

        builder.HasIndex(m => m.UserId)
            .HasDatabaseName("ix_group_members_user_id");
    }
}
