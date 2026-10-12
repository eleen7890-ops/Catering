using Application.Abstractions;

namespace Application.Bookings;

public sealed class BookingHistoryService
{
    private readonly IBookingRepository _bookings;

    public BookingHistoryService(IBookingRepository bookings) =>
        _bookings = bookings ?? throw new ArgumentNullException(nameof(bookings));

    public async Task<IReadOnlyList<BookingSummary>> ListAsync(
        string customerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("A signed-in customer is required.", nameof(customerId));

        var history = await _bookings.ListForCustomerAsync(customerId, cancellationToken);
        return history.Where(booking => booking.CustomerId == customerId)
            .OrderByDescending(booking => booking.CreatedAt)
            .Select(booking => new BookingSummary(
                booking.Id, booking.EventDate, booking.EventType, booking.Status,
                booking.GuestCount, booking.GrandTotal, booking.Currency,
                booking.MenuItems.OrderBy(item => item.Category)
                    .Select(item => new BookedMenuItem(item.Name, item.Category, item.PricePerGuest))
                    .ToArray()))
            .ToArray();
    }
}
