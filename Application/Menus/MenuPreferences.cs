using Domain;

namespace Application.Menus;

public sealed record MenuPreferences(
    EventType EventType,
    Cuisine Cuisine,
    DietaryPreference DietaryPreference,
    TastePreference Taste,
    Mood Mood,
    int GuestCount,
    decimal BudgetPerGuest,
    string Currency);
