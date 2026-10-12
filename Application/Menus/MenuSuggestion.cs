using Domain;

namespace Application.Menus;

public sealed record SuggestedMenuItem(
    Guid Id,
    string Name,
    string Description,
    MenuCategory Category,
    decimal PricePerGuest);

public sealed record MenuSuggestion(
    SuggestedMenuItem Main,
    SuggestedMenuItem Side,
    SuggestedMenuItem Dessert,
    int GuestCount,
    decimal PricePerGuest,
    string Currency)
{
    public decimal EstimatedFoodTotal => PricePerGuest * GuestCount;
}
