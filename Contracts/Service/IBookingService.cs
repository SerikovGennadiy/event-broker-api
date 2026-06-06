using Shared.DTO;

namespace Contracts.Service;

public interface IBookingService
{
    Task<ICollection<BookingDTO>> GetPendingBookingsAsync(CancellationToken cancellationToken = default);
    Task ConfirmBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task RejectBooingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<BookingDTO> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<BookingDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
