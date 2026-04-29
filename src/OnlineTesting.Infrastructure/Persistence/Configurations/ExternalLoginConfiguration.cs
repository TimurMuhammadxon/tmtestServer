using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.Provider).HasConversion<int>().IsRequired();
        builder.Property(e => e.ExternalUserId).IsRequired().HasMaxLength(64);
        builder.Property(e => e.ExternalUsername).HasMaxLength(64);
        builder.Property(e => e.LinkedAt).IsRequired();

        // Один external account → один наш юзер
        builder.HasIndex(e => new { e.Provider, e.ExternalUserId })
            .IsUnique();

        // Один наш юзер → один account у каждого provider'а
        builder.HasIndex(e => new { e.UserId, e.Provider })
            .IsUnique();
    }
}