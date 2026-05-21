using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Payments;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class PaymentOrderConfiguration : IEntityTypeConfiguration<PaymentOrder>
{
    public void Configure(EntityTypeBuilder<PaymentOrder> builder)
    {
        builder.ToTable("payment_orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.PlanId).IsRequired();
        builder.Property(o => o.AmountTiyin).IsRequired();
        builder.Property(o => o.Status).IsRequired();
        builder.Property(o => o.CreatedAt).IsRequired();

        builder.HasIndex(o => o.UserId).HasDatabaseName("ix_payment_orders_user_id");
        builder.HasIndex(o => o.Status).HasDatabaseName("ix_payment_orders_status");
    }
}
