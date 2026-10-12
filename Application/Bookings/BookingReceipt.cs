using Domain;

namespace Application.Bookings;

public sealed record BookingReceipt(
    Guid BookingId,
    BookingStatus Status,
    decimal FoodTotal,
    decimal DeliveryFee,
    decimal DiscountAmount,
    decimal GrandTotal,
    string Currency);
