using Application.Abstractions;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class EfBookingRepository(CateringDbContext context) : IBookingRepository
{
    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(booking);
        await context.Bookings.AddAsync(booking, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountCompletedForCustomerAsync(string customerId,
        CancellationToken cancellationToken = default) =>
        context.Bookings.AsNoTracking().CountAsync(
            booking => booking.CustomerId == customerId && booking.Status == BookingStatus.Completed,
            cancellationToken);

    public async Task<IReadOnlyList<Booking>> ListForCustomerAsync(string customerId,
        CancellationToken cancellationToken = default) =>
        await context.Bookings.AsNoTracking()
            .Where(booking => booking.CustomerId == customerId)
            .Include(booking => booking.MenuItems)
            .OrderByDescending(booking => booking.CreatedAt)
            .ToListAsync(cancellationToken);
}
