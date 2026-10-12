using Application.Abstractions;
using Domain;

namespace Application.Menus;

public sealed class MenuRecommendationService
{
    private readonly IMenuItemRepository _menuItems;

    public MenuRecommendationService(IMenuItemRepository menuItems) =>
        _menuItems = menuItems ?? throw new ArgumentNullException(nameof(menuItems));

    public async Task<MenuSuggestion?> RecommendAsync(
        MenuPreferences preferences, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        Validate(preferences);

        var currency = preferences.Currency.Trim().ToUpperInvariant();
        var available = await _menuItems.GetAvailableAsync(cancellationToken);
        var eligible = available.Where(item => item.IsAvailable &&
            item.PricePerGuest > 0 &&
            string.Equals(item.Currency, currency, StringComparison.OrdinalIgnoreCase) &&
            (item.Cuisine == preferences.Cuisine || item.Cuisine == Cuisine.International) &&
            FitsDiet(item, preferences.DietaryPreference)).ToArray();

        var candidates = from main in eligible.Where(item => item.Category == MenuCategory.Main)
                         from side in eligible.Where(item => item.Category == MenuCategory.Side)
                         from dessert in eligible.Where(item => item.Category == MenuCategory.Dessert)
                         let price = main.PricePerGuest + side.PricePerGuest + dessert.PricePerGuest
                         where price <= preferences.BudgetPerGuest
                         select new
                         {
                             Main = main,
                             Side = side,
                             Dessert = dessert,
                             Price = price,
                             Score = Score(main, preferences) + Score(side, preferences) + Score(dessert, preferences)
                         };

        var best = candidates.OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Price)
            .ThenBy(candidate => candidate.Main.Name)
            .ThenBy(candidate => candidate.Side.Name)
            .ThenBy(candidate => candidate.Dessert.Name)
            .FirstOrDefault();

        return best is null ? null : new MenuSuggestion(
            Snapshot(best.Main), Snapshot(best.Side), Snapshot(best.Dessert),
            preferences.GuestCount, best.Price, currency);
    }

    private static SuggestedMenuItem Snapshot(MenuItem item) =>
        new(item.Id, item.Name, item.Description, item.Category, item.PricePerGuest);

    private static bool FitsDiet(MenuItem item, DietaryPreference preference) => preference switch
    {
        DietaryPreference.Any => true,
        DietaryPreference.Vegetarian => item.DietarySuitability is
            DietarySuitability.Vegetarian or DietarySuitability.Vegan,
        DietaryPreference.Vegan => item.DietarySuitability == DietarySuitability.Vegan,
        _ => false
    };

    private static int Score(MenuItem item, MenuPreferences preferences) =>
        (item.Cuisine == preferences.Cuisine ? 3 : 0) +
        (item.Taste == preferences.Taste ? 2 : 0) +
        (item.Mood == preferences.Mood ? 2 : 0) +
        (item.BestForEvent == preferences.EventType ? 2 : 0);

    private static void Validate(MenuPreferences preferences)
    {
        if (!Enum.IsDefined(preferences.EventType) || !Enum.IsDefined(preferences.Cuisine) ||
            !Enum.IsDefined(preferences.DietaryPreference) || !Enum.IsDefined(preferences.Taste) ||
            !Enum.IsDefined(preferences.Mood))
            throw new ArgumentException("One or more preferences are invalid.", nameof(preferences));
        if (preferences.GuestCount < 1 || preferences.BudgetPerGuest <= 0)
            throw new ArgumentException("Guest count and budget must be positive.", nameof(preferences));
        if (string.IsNullOrWhiteSpace(preferences.Currency) ||
            preferences.Currency.Trim().Length != 3 ||
            !preferences.Currency.Trim().All(character =>
                character is >= 'A' and <= 'Z' or >= 'a' and <= 'z'))
            throw new ArgumentException("Use a three-letter currency code.", nameof(preferences));
    }
}
