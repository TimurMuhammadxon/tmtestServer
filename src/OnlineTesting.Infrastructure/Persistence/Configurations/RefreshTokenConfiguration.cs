using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(128);
        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.RevokedAt);
        builder.Property(t => t.ReplacedByTokenHash).HasMaxLength(128);
        builder.Property(t => t.CreatedByIp).HasMaxLength(64);

        // Concurrency через системную колонку xmin (PostgreSQL row version).
        // Маппим property RowVersion на xmin — БД обновляет его автоматически на каждый UPDATE.
        // Никакой миграции для xmin не нужно: эта колонка существует в любой Postgres-таблице.
        builder.Property(t => t.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasIndex(t => t.TokenHash);
        builder.HasIndex(t => t.UserId);

        builder.Ignore(t => t.IsActive);
    }
}