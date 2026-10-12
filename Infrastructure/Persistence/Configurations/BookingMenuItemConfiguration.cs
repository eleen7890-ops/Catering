using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class BookingMenuItemConfiguration : IEntityTypeConfiguration<BookingMenuItem>
{
    public void Configure(EntityTypeBuilder<BookingMenuItem> builder)
    {
        builder.ToTable("BookingMenuItems");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Name).IsRequired().HasMaxLength(150);
        builder.Property(item => item.Category).HasConversion<string>().HasMaxLength(20);
        builder.Property(item => item.PricePerGuest).HasPrecision(18, 2);
        // MenuItemId is a historical dish reference, not a foreign key. Deleting an
        // old catalog item or curated menu must not change a booking snapshot.
    }
}
