using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Subscriptions;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.PlanId).IsRequired();
        builder.Property(s => s.StartsAt).IsRequired();
        builder.Property(s => s.ExpiresAt).IsRequired();

        // One active subscription row per user
        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("ux_subscriptions_user_id")
            .IsUnique();

        builder.HasIndex(s => s.ExpiresAt)
            .HasDatabaseName("ix_subscriptions_expires_at");
    }
}
