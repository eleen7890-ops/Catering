using Domain;

namespace Application.Bookings;

public sealed record BookingRequest(
    Guid MenuId,
    EventType EventType,
    DietaryPreference DietaryPreference,
    TastePreference Taste,
    int GuestCount,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    DateTimeOffset EventDate,
    FulfillmentMethod FulfillmentMethod,
    string? DeliveryAddress,
    string? CakeDescription = null,
    CakeImageUpload? CakeImage = null);

// The web layer reads an uploaded file into this value before creating a booking.
public sealed record CakeImageUpload(string ContentType, byte[] Data);
