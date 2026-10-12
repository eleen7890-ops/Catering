namespace Domain;

public class MenuItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MenuCategory Category { get; set; }
    public Cuisine Cuisine { get; set; }
    public DietarySuitability DietarySuitability { get; set; }
    public TastePreference Taste { get; set; }
    public Mood Mood { get; set; }
    public EventType? BestForEvent { get; set; }
    public decimal PricePerGuest { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}
