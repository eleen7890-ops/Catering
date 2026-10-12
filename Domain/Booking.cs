namespace Domain;

public class Booking
{
    public Guid Id { get; set; }
    public string? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTimeOffset EventDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public EventType EventType { get; set; }
    public Guid? CuratedMenuId { get; set; }
    public string? CuratedMenuName { get; set; }
    public string? CakeDescription { get; set; }
    public string? CakeImageContentType { get; set; }
    public byte[]? CakeImageData { get; set; }
    public Cuisine Cuisine { get; set; }
    public DietaryPreference DietaryPreference { get; set; }
    public TastePreference Taste { get; set; }
    public Mood Mood { get; set; }
    public int GuestCount { get; set; }
    public decimal BudgetPerGuest { get; set; }
    public string Currency { get; set; } = string.Empty;
    public FulfillmentMethod FulfillmentMethod { get; set; }
    public string? DeliveryAddress { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal FoodTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public BookingStatus Status { get; set; }
    public ICollection<BookingMenuItem> MenuItems { get; set; } = new List<BookingMenuItem>();
}

public class BookingMenuItem
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public MenuCategory Category { get; set; }
    public decimal PricePerGuest { get; set; }
}
