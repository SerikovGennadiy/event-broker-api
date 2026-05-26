using Shared.DTO;

namespace Contracts.Service;

public interface IBookingService
{
    ICollection<BookingDTO> GetPendingBookings();
    void ConfirmBooking(Guid bookingId);
    void RejectBooking(Guid bookingId);
    Task<BookingDTO> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<BookingDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
