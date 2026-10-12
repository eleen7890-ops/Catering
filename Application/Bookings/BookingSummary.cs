using Domain;

namespace Application.Bookings;

public sealed record BookedMenuItem(string Name, MenuCategory Category, decimal PricePerGuest);

public sealed record BookingSummary(
    Guid BookingId,
    DateTimeOffset EventDate,
    EventType EventType,
    BookingStatus Status,
    int GuestCount,
    decimal GrandTotal,
    string Currency,
    IReadOnlyList<BookedMenuItem> MenuItems);
