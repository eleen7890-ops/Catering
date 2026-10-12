using Domain;

namespace Application.Abstractions;

public sealed record CuratedMenuQuery(Cuisine? Cuisine = null, Mood? Vibe = null, EventType? Event = null);

public interface ICuratedMenuRepository
{
    Task<IReadOnlyList<CuratedMenu>> ListPublishedAsync(CuratedMenuQuery query, int? limit = null,
        CancellationToken cancellationToken = default);
    Task<CuratedMenu?> GetAsync(Guid id, bool includeUnpublished = false,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CuratedMenu>> ListAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(CuratedMenu menu, CancellationToken cancellationToken = default);
    Task UpdateAsync(CuratedMenu menu, CancellationToken cancellationToken = default);
    Task DeleteAsync(CuratedMenu menu, CancellationToken cancellationToken = default);
}
