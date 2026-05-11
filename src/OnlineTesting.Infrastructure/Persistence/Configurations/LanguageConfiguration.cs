using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.HasKey(l => l.Code);

        builder.Property(l => l.Code).IsRequired().HasMaxLength(20);
        builder.Property(l => l.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(l => l.IsDefault).IsRequired();
        builder.Property(l => l.IsActive).IsRequired();

        builder.HasIndex(l => l.IsDefault)
            .HasFilter("is_default = true")
            .IsUnique()
            .HasDatabaseName("ux_languages_default");
    }
}