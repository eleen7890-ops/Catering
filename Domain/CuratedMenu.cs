namespace Domain;

public class CuratedMenu
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public decimal PriceJod { get; set; }
    public int MinimumGuests { get; set; }
    public int MaximumGuests { get; set; }
    public Cuisine Cuisine { get; set; }
    public Mood Vibe { get; set; }
    public EventType Event { get; set; }
    public bool IsPublished { get; set; }
    public ICollection<CuratedMenuDish> Dishes { get; set; } = new List<CuratedMenuDish>();
}

public class CuratedMenuDish
{
    public Guid Id { get; set; }
    public Guid CuratedMenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ServingDescription { get; set; } = string.Empty;
    public MenuCategory Category { get; set; }
    public int SortOrder { get; set; }
}
