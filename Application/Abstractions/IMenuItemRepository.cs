using Domain;

namespace Application.Abstractions;

public interface IMenuItemRepository
{
    Task<IReadOnlyList<MenuItem>> GetAvailableAsync(CancellationToken cancellationToken = default);
}
