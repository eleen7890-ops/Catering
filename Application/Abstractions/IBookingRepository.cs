using Domain;

namespace Application.Abstractions;

public interface IBookingRepository
{
    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);
    Task<int> CountCompletedForCustomerAsync(string customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Booking>> ListForCustomerAsync(string customerId, CancellationToken cancellationToken = default);
}
