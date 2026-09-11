namespace Bookings.Application.Contracts.Persistence;

public interface IRepositoryManager
{
    IBookingRepository Booking { get; }
    IEventReadRepository EventRead { get; }
    Task SaveAsync(CancellationToken cancellationToken = default);
}
