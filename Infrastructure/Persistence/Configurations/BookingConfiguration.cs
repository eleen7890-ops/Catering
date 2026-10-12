using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(booking => booking.Id);
        builder.Property(booking => booking.CustomerId).HasMaxLength(450);
        builder.Property(booking => booking.CustomerName).IsRequired().HasMaxLength(150);
        builder.Property(booking => booking.CustomerEmail).IsRequired().HasMaxLength(256);
        builder.Property(booking => booking.CustomerPhone).IsRequired().HasMaxLength(40);
        builder.Property(booking => booking.EventType).HasConversion<string>().HasMaxLength(30);
        builder.Property(booking => booking.CakeDescription).HasMaxLength(1000);
        builder.Property(booking => booking.CakeImageContentType).HasMaxLength(30).IsUnicode(false);
        builder.Property(booking => booking.CakeImageData).HasColumnType("varbinary(max)");
        builder.Property(booking => booking.Cuisine).HasConversion<string>().HasMaxLength(30);
        builder.Property(booking => booking.DietaryPreference).HasConversion<string>().HasMaxLength(20);
        builder.Property(booking => booking.Taste).HasConversion<string>().HasMaxLength(20);
        builder.Property(booking => booking.Mood).HasConversion<string>().HasMaxLength(20);
        builder.Property(booking => booking.FulfillmentMethod).HasConversion<string>().HasMaxLength(20);
        builder.Property(booking => booking.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(booking => booking.Currency).IsRequired().HasMaxLength(3).IsUnicode(false);
        builder.Property(booking => booking.DeliveryAddress).HasMaxLength(500);
        builder.Property(booking => booking.BudgetPerGuest).HasPrecision(18, 2);
        builder.Property(booking => booking.DeliveryFee).HasPrecision(18, 2);
        builder.Property(booking => booking.FoodTotal).HasPrecision(18, 2);
        builder.Property(booking => booking.DiscountAmount).HasPrecision(18, 2);
        builder.Property(booking => booking.GrandTotal).HasPrecision(18, 2);

        builder.HasOne<IdentityUser>().WithMany().HasForeignKey(booking => booking.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(booking => booking.MenuItems).WithOne()
            .HasForeignKey(item => item.BookingId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(booking => new { booking.CustomerId, booking.CreatedAt });
        builder.HasIndex(booking => new { booking.CustomerId, booking.Status });
    }
}
