using Shared.DTO;

namespace Contracts.Service;

public interface IBookingService
{
    Task<BookingDTO> GetBookingByIdAsync(Guid bookingId);
    Task<ICollection<BookingDTO>> GetPendingBookingsAsync();
    Task ConfirmBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task RejectBooingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<BookingDTO> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken = default);
}
