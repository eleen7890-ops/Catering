using Application.Menus;
using Domain;

namespace Web.Models;

public sealed class MenuListViewModel
{
    public Cuisine? Cuisine { get; init; }
    public Mood? Vibe { get; init; }
    public EventType? Event { get; init; }
    public required IReadOnlyList<CuratedMenuView> Menus { get; init; }
    public bool CatalogUnavailable { get; init; }
}
