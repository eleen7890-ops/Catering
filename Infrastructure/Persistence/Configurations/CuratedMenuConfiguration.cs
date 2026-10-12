using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CuratedMenuConfiguration : IEntityTypeConfiguration<CuratedMenu>
{
    public void Configure(EntityTypeBuilder<CuratedMenu> builder)
    {
        builder.ToTable("CuratedMenus");
        builder.HasKey(menu => menu.Id);
        builder.Property(menu => menu.Name).IsRequired().HasMaxLength(150);
        builder.Property(menu => menu.Description).IsRequired().HasMaxLength(1500);
        builder.Property(menu => menu.ImagePath).IsRequired().HasMaxLength(500);
        builder.Property(menu => menu.PriceJod).HasPrecision(18, 2);
        builder.Property(menu => menu.Cuisine).HasConversion<string>().HasMaxLength(30);
        builder.Property(menu => menu.Vibe).HasConversion<string>().HasMaxLength(20);
        builder.Property(menu => menu.Event).HasConversion<string>().HasMaxLength(30);
        builder.HasMany(menu => menu.Dishes).WithOne().HasForeignKey(dish => dish.CuratedMenuId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(menu => new { menu.IsPublished, menu.Cuisine, menu.Vibe, menu.Event });
    }
}

public sealed class CuratedMenuDishConfiguration : IEntityTypeConfiguration<CuratedMenuDish>
{
    public void Configure(EntityTypeBuilder<CuratedMenuDish> builder)
    {
        builder.ToTable("CuratedMenuDishes");
        builder.HasKey(dish => dish.Id);
        builder.Property(dish => dish.Name).IsRequired().HasMaxLength(150);
        builder.Property(dish => dish.ServingDescription).IsRequired().HasMaxLength(120);
        builder.Property(dish => dish.Category).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(dish => new { dish.CuratedMenuId, dish.SortOrder });
    }
}
