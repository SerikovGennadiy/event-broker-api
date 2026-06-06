using Shared.DTO;

namespace Contracts.Service;

public interface IBookingService
{
    Task<BookingDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<ICollection<BookingDTO>> GetPendingBookingsAsync(CancellationToken cancellationToken = default);
    Task ConfirmBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task RejectBooingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    BookingDTO CreateBooking(Guid eventId);
}
