using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Subscriptions;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("subscription_plans");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Type).IsRequired();
        builder.Property(p => p.Duration).IsRequired();
        builder.Property(p => p.Price).HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(p => p.IsActive).IsRequired();

        builder.HasIndex(p => new { p.Type, p.Duration })
            .HasDatabaseName("ux_subscription_plans_type_duration")
            .IsUnique();
    }
}
