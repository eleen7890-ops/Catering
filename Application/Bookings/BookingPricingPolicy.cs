using Domain;

namespace Application.Bookings;

public sealed class BookingPricingPolicy
{
    private readonly Dictionary<string, decimal> _deliveryFees;

    public BookingPricingPolicy(int completedBookingsForDiscount, decimal loyaltyDiscountRate,
        IReadOnlyDictionary<string, decimal> deliveryFees)
    {
        if (completedBookingsForDiscount < 1)
            throw new ArgumentOutOfRangeException(nameof(completedBookingsForDiscount));
        if (loyaltyDiscountRate is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(loyaltyDiscountRate));
        ArgumentNullException.ThrowIfNull(deliveryFees);

        _deliveryFees = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var (currency, fee) in deliveryFees)
        {
            if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3 ||
                !currency.Trim().All(character => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z') ||
                fee < 0)
                throw new ArgumentException("Delivery fees need a three-letter currency and a nonnegative amount.", nameof(deliveryFees));
            _deliveryFees.Add(currency.Trim(), fee);
        }

        CompletedBookingsForDiscount = completedBookingsForDiscount;
        LoyaltyDiscountRate = loyaltyDiscountRate;
    }

    public int CompletedBookingsForDiscount { get; }
    public decimal LoyaltyDiscountRate { get; }

    public decimal DeliveryFee(FulfillmentMethod method, string currency)
    {
        if (method == FulfillmentMethod.Pickup) return 0;
        if (method != FulfillmentMethod.Delivery) throw new ArgumentOutOfRangeException(nameof(method));
        return _deliveryFees.TryGetValue(currency, out var fee)
            ? fee
            : throw new InvalidOperationException($"No delivery fee is configured for {currency}.");
    }

    public decimal Discount(decimal foodTotal, int completedBookings, bool isRegisteredCustomer)
    {
        if (foodTotal < 0) throw new ArgumentOutOfRangeException(nameof(foodTotal));
        if (completedBookings < 0) throw new ArgumentOutOfRangeException(nameof(completedBookings));
        return isRegisteredCustomer && completedBookings >= CompletedBookingsForDiscount
            ? decimal.Round(foodTotal * LoyaltyDiscountRate, 2, MidpointRounding.AwayFromZero)
            : 0;
    }
}
