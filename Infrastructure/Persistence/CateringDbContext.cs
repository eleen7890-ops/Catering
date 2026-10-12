using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class CateringDbContext(DbContextOptions<CateringDbContext> options) : IdentityDbContext(options)
{
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingMenuItem> BookingMenuItems => Set<BookingMenuItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(CateringDbContext).Assembly);
    }
}
