using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Payments;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class ClickTransactionConfiguration : IEntityTypeConfiguration<ClickTransaction>
{
    public void Configure(EntityTypeBuilder<ClickTransaction> builder)
    {
        builder.ToTable("click_transactions");
        builder.HasKey(t => t.Id);

        // Auto-incremented numeric ID returned to Click as merchant_prepare_id
        builder.Property(t => t.PrepareId)
            .ValueGeneratedOnAdd()
            .UseIdentityAlwaysColumn();

        builder.Property(t => t.ClickTransactionId).IsRequired().HasMaxLength(64);
        builder.Property(t => t.OrderId).IsRequired();
        builder.Property(t => t.Amount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(t => t.State).IsRequired();
        builder.Property(t => t.PrepareTime).IsRequired();
        builder.Property(t => t.CompleteTime);
        builder.Property(t => t.Error);

        builder.HasIndex(t => t.ClickTransactionId)
            .HasDatabaseName("ux_click_transactions_click_id")
            .IsUnique();

        builder.HasIndex(t => t.PrepareId)
            .HasDatabaseName("ux_click_transactions_prepare_id")
            .IsUnique();

        builder.HasIndex(t => t.OrderId)
            .HasDatabaseName("ix_click_transactions_order_id");
    }
}
