using System.Net.Mail;
using Application.Abstractions;
using Application.Menus;
using Domain;

namespace Application.Bookings;

public sealed class BookingService
{
    private readonly MenuRecommendationService _recommendations;
    private readonly IBookingRepository _bookings;
    private readonly BookingPricingPolicy _pricing;
    private readonly TimeProvider _clock;

    public BookingService(MenuRecommendationService recommendations, IBookingRepository bookings,
        BookingPricingPolicy pricing, TimeProvider clock)
    {
        _recommendations = recommendations ?? throw new ArgumentNullException(nameof(recommendations));
        _bookings = bookings ?? throw new ArgumentNullException(nameof(bookings));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    // Returns null when no complete menu fits the customer's preferences and budget.
    public async Task<BookingReceipt?> CreateAsync(BookingRequest request, string? customerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validate(request, customerId);

        // Recalculate from the current catalog; a browser-supplied price is never trusted.
        var menu = await _recommendations.RecommendAsync(request.Preferences, cancellationToken);
        if (menu is null) return null;

        var completedBookings = customerId is null ? 0 :
            await _bookings.CountCompletedForCustomerAsync(customerId, cancellationToken);
        var foodTotal = menu.EstimatedFoodTotal;
        var deliveryFee = _pricing.DeliveryFee(request.FulfillmentMethod, menu.Currency);
        var discount = _pricing.Discount(foodTotal, completedBookings, customerId is not null);

        var bookingId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = bookingId,
            CustomerId = customerId,
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = MailAddress.TryCreate(request.CustomerEmail, out var email)
                ? email.Address : request.CustomerEmail.Trim(),
            CustomerPhone = request.CustomerPhone.Trim(),
            EventDate = request.EventDate,
            CreatedAt = _clock.GetUtcNow(),
            EventType = request.Preferences.EventType,
            CakeDescription = request.Preferences.EventType == EventType.Birthday
                ? request.CakeDescription?.Trim() : null,
            CakeImageContentType = request.CakeImage?.ContentType,
            CakeImageData = request.CakeImage?.Data.ToArray(),
            Cuisine = request.Preferences.Cuisine,
            DietaryPreference = request.Preferences.DietaryPreference,
            Taste = request.Preferences.Taste,
            Mood = request.Preferences.Mood,
            GuestCount = menu.GuestCount,
            BudgetPerGuest = request.Preferences.BudgetPerGuest,
            Currency = menu.Currency,
            FulfillmentMethod = request.FulfillmentMethod,
            DeliveryAddress = request.FulfillmentMethod == FulfillmentMethod.Delivery
                ? request.DeliveryAddress!.Trim() : null,
            DeliveryFee = deliveryFee,
            FoodTotal = foodTotal,
            DiscountAmount = discount,
            GrandTotal = foodTotal + deliveryFee - discount,
            Status = BookingStatus.Requested,
            MenuItems =
            [
                Snapshot(menu.Main, bookingId),
                Snapshot(menu.Side, bookingId),
                Snapshot(menu.Dessert, bookingId)
            ]
        };

        await _bookings.AddAsync(booking, cancellationToken);
        return new BookingReceipt(booking.Id, booking.Status, booking.FoodTotal,
            booking.DeliveryFee, booking.DiscountAmount, booking.GrandTotal, booking.Currency);
    }

    private static BookingMenuItem Snapshot(SuggestedMenuItem item, Guid bookingId) => new()
    {
        Id = Guid.NewGuid(),
        BookingId = bookingId,
        MenuItemId = item.Id,
        Name = item.Name,
        Category = item.Category,
        PricePerGuest = item.PricePerGuest
    };

    private void Validate(BookingRequest request, string? customerId)
    {
        if (request.Preferences is null)
            throw new ArgumentException("Menu preferences are required.", nameof(request));
        if (request.CakeDescription is { Length: > 1000 })
            throw new ArgumentException("Cake description cannot exceed 1000 characters.", nameof(request));
        if (request.Preferences.EventType != EventType.Birthday &&
            (!string.IsNullOrWhiteSpace(request.CakeDescription) || request.CakeImage is not null))
            throw new ArgumentException("Cake details are only available for birthdays.", nameof(request));
        if (request.CakeImage is not null)
            ValidateCakeImage(request.CakeImage);
        if (string.IsNullOrWhiteSpace(request.CustomerName) ||
            string.IsNullOrWhiteSpace(request.CustomerPhone) ||
            !MailAddress.TryCreate(request.CustomerEmail, out _))
            throw new ArgumentException("Valid customer contact details are required.", nameof(request));
        if (request.EventDate <= _clock.GetUtcNow())
            throw new ArgumentException("The event date must be in the future.", nameof(request));
        if (!Enum.IsDefined(request.FulfillmentMethod))
            throw new ArgumentException("The fulfillment method is invalid.", nameof(request));
        if (request.FulfillmentMethod == FulfillmentMethod.Delivery &&
            string.IsNullOrWhiteSpace(request.DeliveryAddress))
            throw new ArgumentException("A delivery address is required.", nameof(request));
        if (customerId is not null && string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID cannot be blank.", nameof(customerId));
    }

    private static void ValidateCakeImage(CakeImageUpload image)
    {
        const int maxImageBytes = 5 * 1024 * 1024;
        if (image.Data is null || image.Data.Length == 0 || image.Data.Length > maxImageBytes)
            throw new ArgumentException("Cake image must be between 1 byte and 5 MB.", nameof(image));

        var data = image.Data;
        var valid = image.ContentType switch
        {
            "image/jpeg" => data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF,
            "image/png" => data.Length >= 8 && data.AsSpan(0, 8).SequenceEqual(
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            "image/webp" => data.Length >= 12 && data.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
                data.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
        if (!valid)
            throw new ArgumentException("Cake image must be a JPEG, PNG, or WebP file.", nameof(image));
    }
}
