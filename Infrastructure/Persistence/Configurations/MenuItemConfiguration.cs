using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Name).IsRequired().HasMaxLength(150);
        builder.Property(item => item.Description).IsRequired().HasMaxLength(1000);
        builder.Property(item => item.Category).HasConversion<string>().HasMaxLength(20);
        builder.Property(item => item.Cuisine).HasConversion<string>().HasMaxLength(30);
        builder.Property(item => item.DietarySuitability).HasConversion<string>().HasMaxLength(20);
        builder.Property(item => item.Taste).HasConversion<string>().HasMaxLength(20);
        builder.Property(item => item.Mood).HasConversion<string>().HasMaxLength(20);
        builder.Property(item => item.BestForEvent).HasConversion<string>().HasMaxLength(30);
        builder.Property(item => item.PricePerGuest).HasPrecision(18, 2);
        builder.Property(item => item.Currency).IsRequired().HasMaxLength(3).IsUnicode(false);
        builder.HasIndex(item => new { item.IsAvailable, item.Currency, item.Category });
    }
}
