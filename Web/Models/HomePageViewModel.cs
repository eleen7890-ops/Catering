using Domain;

namespace Web.Models;

public sealed record OccasionCard(string Name, string Description, string Eyebrow, string Style,
    Mood? Vibe = null, EventType? Event = null);

public sealed record FeaturedPackage(
    Guid Id,
    string Name,
    string Description,
    string ImagePath,
    string ImageAlt,
    string Badge,
    decimal PriceJod,
    int MinimumGuests,
    int MaximumGuests,
    IReadOnlyList<string> IncludedDishes);

public sealed class HomePageViewModel
{
    public required IReadOnlyList<OccasionCard> Occasions { get; init; }
    public required IReadOnlyList<FeaturedPackage> Packages { get; init; }
    public bool CatalogUnavailable { get; init; }
}
