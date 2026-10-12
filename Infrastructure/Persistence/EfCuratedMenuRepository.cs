using Application.Abstractions;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class EfCuratedMenuRepository(CateringDbContext context) : ICuratedMenuRepository
{
    public async Task<IReadOnlyList<CuratedMenu>> ListPublishedAsync(CuratedMenuQuery query, int? limit = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<CuratedMenu> menus = context.CuratedMenus.AsNoTracking().Where(menu => menu.IsPublished);
        if (query.Cuisine is { } cuisine) menus = menus.Where(menu => menu.Cuisine == cuisine);
        if (query.Vibe is { } vibe) menus = menus.Where(menu => menu.Vibe == vibe);
        if (query.Event is { } eventType) menus = menus.Where(menu => menu.Event == eventType);
        menus = menus.OrderBy(menu => menu.Name).ThenBy(menu => menu.Id).Include(menu => menu.Dishes);
        if (limit is > 0) menus = menus.Take(limit.Value);
        return await menus.ToListAsync(cancellationToken);
    }

    public Task<CuratedMenu?> GetAsync(Guid id, bool includeUnpublished = false,
        CancellationToken cancellationToken = default) =>
        context.CuratedMenus.AsNoTracking().Include(menu => menu.Dishes)
            .FirstOrDefaultAsync(menu => menu.Id == id && (includeUnpublished || menu.IsPublished), cancellationToken);

    public async Task<IReadOnlyList<CuratedMenu>> ListAllAsync(CancellationToken cancellationToken = default) =>
        await context.CuratedMenus.AsNoTracking().Include(menu => menu.Dishes)
            .OrderBy(menu => menu.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(CuratedMenu menu, CancellationToken cancellationToken = default)
    {
        context.CuratedMenus.Add(menu);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CuratedMenu menu, CancellationToken cancellationToken = default)
    {
        var current = await context.CuratedMenus.Include(item => item.Dishes)
            .FirstOrDefaultAsync(item => item.Id == menu.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Menu not found.");
        context.CuratedMenuDishes.RemoveRange(current.Dishes);
        current.Name = menu.Name;
        current.Description = menu.Description;
        current.ImagePath = menu.ImagePath;
        current.PriceJod = menu.PriceJod;
        current.MinimumGuests = menu.MinimumGuests;
        current.MaximumGuests = menu.MaximumGuests;
        current.Cuisine = menu.Cuisine;
        current.Vibe = menu.Vibe;
        current.Event = menu.Event;
        current.IsPublished = menu.IsPublished;
        current.Dishes = menu.Dishes;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(CuratedMenu menu, CancellationToken cancellationToken = default)
    {
        context.CuratedMenus.Remove(menu);
        await context.SaveChangesAsync(cancellationToken);
    }
}
