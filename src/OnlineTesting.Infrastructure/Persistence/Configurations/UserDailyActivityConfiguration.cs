using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Progress;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class UserDailyActivityConfiguration : IEntityTypeConfiguration<UserDailyActivity>
{
    public void Configure(EntityTypeBuilder<UserDailyActivity> builder)
    {
        builder.ToTable("user_daily_activities");
        builder.HasKey(a => new { a.UserId, a.ActivityDate });
        builder.Property(a => a.UserId).HasColumnName("user_id");
        builder.Property(a => a.ActivityDate).HasColumnName("activity_date");
        builder.HasIndex(a => a.UserId).HasDatabaseName("ix_user_daily_activities_user_id");
    }
}
