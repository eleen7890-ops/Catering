using Application.Abstractions;
using Domain;

namespace Application.Menus;

public sealed record CuratedMenuDishView(Guid Id, string Name, string ServingDescription, MenuCategory Category, int SortOrder);
public sealed record CuratedMenuView(
    Guid Id, string Name, string Description, string ImagePath, decimal PriceJod,
    int MinimumGuests, int MaximumGuests, Cuisine Cuisine, Mood Vibe, EventType Event,
    bool IsPublished, IReadOnlyList<CuratedMenuDishView> Dishes);
public sealed record CuratedMenuDishInput(string Name, string ServingDescription, MenuCategory Category);
public sealed record SaveCuratedMenu(
    string Name, string Description, string ImagePath, decimal PriceJod,
    int MinimumGuests, int MaximumGuests, Cuisine Cuisine, Mood Vibe, EventType Event,
    bool IsPublished, IReadOnlyList<CuratedMenuDishInput> Dishes);

public sealed class CuratedMenuService(ICuratedMenuRepository repository)
{
    public async Task<IReadOnlyList<CuratedMenuView>> ListPublishedAsync(CuratedMenuQuery query,
        int? limit = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.Cuisine is { } cuisine && !Enum.IsDefined(cuisine) ||
            query.Vibe is { } vibe && !Enum.IsDefined(vibe) ||
            query.Event is { } eventType && !Enum.IsDefined(eventType))
            throw new ArgumentException("Invalid menu filter.", nameof(query));
        var menus = await repository.ListPublishedAsync(query, limit, cancellationToken);
        return menus.Select(Map).ToArray();
    }

    public async Task<CuratedMenuView?> GetPublishedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var menu = await repository.GetAsync(id, cancellationToken: cancellationToken);
        return menu is null ? null : Map(menu);
    }

    public async Task<CuratedMenuView?> GetForAdminAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var menu = await repository.GetAsync(id, includeUnpublished: true, cancellationToken);
        return menu is null ? null : Map(menu);
    }

    public async Task<IReadOnlyList<CuratedMenuView>> ListAllForAdminAsync(CancellationToken cancellationToken = default) =>
        (await repository.ListAllAsync(cancellationToken)).Select(Map).ToArray();

    public async Task<Guid> CreateAsync(SaveCuratedMenu input, CancellationToken cancellationToken = default)
    {
        Validate(input);
        var menu = Build(Guid.NewGuid(), input);
        await repository.AddAsync(menu, cancellationToken);
        return menu.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, SaveCuratedMenu input, CancellationToken cancellationToken = default)
    {
        Validate(input);
        if (await repository.GetAsync(id, includeUnpublished: true, cancellationToken) is null) return false;
        await repository.UpdateAsync(Build(id, input), cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var menu = await repository.GetAsync(id, includeUnpublished: true, cancellationToken);
        if (menu is null) return false;
        await repository.DeleteAsync(new CuratedMenu { Id = id }, cancellationToken);
        return true;
    }

    private static CuratedMenu Build(Guid id, SaveCuratedMenu input) => new()
    {
        Id = id,
        Name = input.Name.Trim(),
        Description = input.Description.Trim(),
        ImagePath = input.ImagePath.Trim(),
        PriceJod = input.PriceJod,
        MinimumGuests = input.MinimumGuests,
        MaximumGuests = input.MaximumGuests,
        Cuisine = input.Cuisine,
        Vibe = input.Vibe,
        Event = input.Event,
        IsPublished = input.IsPublished,
        Dishes = input.Dishes.Select((dish, index) => new CuratedMenuDish
        {
            Id = Guid.NewGuid(),
            CuratedMenuId = id,
            Name = dish.Name.Trim(),
            ServingDescription = dish.ServingDescription.Trim(),
            Category = dish.Category,
            SortOrder = index
        }).ToList()
    };

    private static CuratedMenuView Map(CuratedMenu menu) => new(
        menu.Id, menu.Name, menu.Description, menu.ImagePath, menu.PriceJod,
        menu.MinimumGuests, menu.MaximumGuests, menu.Cuisine, menu.Vibe, menu.Event,
        menu.IsPublished,
        menu.Dishes.OrderBy(dish => dish.SortOrder)
            .Select(dish => new CuratedMenuDishView(
                dish.Id, dish.Name, dish.ServingDescription, dish.Category, dish.SortOrder)).ToArray());

    private static void Validate(SaveCuratedMenu input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (string.IsNullOrWhiteSpace(input.Name) || input.Name.Length > 150 ||
            string.IsNullOrWhiteSpace(input.Description) || input.Description.Length > 1500)
            throw new ArgumentException("Enter a menu name and description within their length limits.");
        if (string.IsNullOrWhiteSpace(input.ImagePath) || input.ImagePath.Length > 500 ||
            !input.ImagePath.StartsWith("/images/", StringComparison.OrdinalIgnoreCase) ||
            input.ImagePath.Contains("..", StringComparison.Ordinal) ||
            input.ImagePath.Contains('\\') || input.ImagePath.Contains('?') || input.ImagePath.Contains('#'))
            throw new ArgumentException("Use a local image path beginning with /images/.");
        if (input.PriceJod <= 0 || input.MinimumGuests < 1 || input.MaximumGuests < input.MinimumGuests)
            throw new ArgumentException("Price and guest range must be positive and consistent.");
        if (!Enum.IsDefined(input.Cuisine) || !Enum.IsDefined(input.Vibe) || !Enum.IsDefined(input.Event))
            throw new ArgumentException("Choose valid cuisine, vibe, and event values.");
        if (input.Dishes is null || input.Dishes.Count == 0 ||
            input.Dishes.Any(dish => dish is null || string.IsNullOrWhiteSpace(dish.Name) ||
                dish.Name.Length > 150 || string.IsNullOrWhiteSpace(dish.ServingDescription) ||
                dish.ServingDescription.Length > 120 || !Enum.IsDefined(dish.Category)))
            throw new ArgumentException("Add at least one dish with a name, serving description, and category.");
    }
}
