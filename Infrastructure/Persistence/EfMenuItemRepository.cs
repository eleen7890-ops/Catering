using Application.Abstractions;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class EfMenuItemRepository(CateringDbContext context) : IMenuItemRepository
{
    public async Task<IReadOnlyList<MenuItem>> GetAvailableAsync(
        CancellationToken cancellationToken = default) =>
        await context.MenuItems.AsNoTracking()
            .Where(item => item.IsAvailable)
            .ToListAsync(cancellationToken);
}
