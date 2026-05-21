using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Payments;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class PaymeTransactionConfiguration : IEntityTypeConfiguration<PaymeTransaction>
{
    public void Configure(EntityTypeBuilder<PaymeTransaction> builder)
    {
        builder.ToTable("payme_transactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.PaymeTransactionId).IsRequired().HasMaxLength(64);
        builder.Property(t => t.OrderId).IsRequired();
        builder.Property(t => t.Amount).IsRequired();
        builder.Property(t => t.State).IsRequired();
        builder.Property(t => t.CreateTime).IsRequired();
        builder.Property(t => t.PerformTime);
        builder.Property(t => t.CancelTime);
        builder.Property(t => t.CancelReason);

        builder.HasIndex(t => t.PaymeTransactionId)
            .HasDatabaseName("ux_payme_transactions_payme_id")
            .IsUnique();

        builder.HasIndex(t => t.OrderId).HasDatabaseName("ix_payme_transactions_order_id");
    }
}
