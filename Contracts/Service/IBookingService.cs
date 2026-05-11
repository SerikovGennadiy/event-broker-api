using Shared.DTO;

namespace Contracts.Service;

public interface IBookingService
{
    IEnumerable<BookingDTO> GetPendingBookings();
    void ConfirmBooking(Guid bookingId);
    void RejectBooking(Guid bookingId);
    BookingDTO CreateBookingAsync(Guid eventId);
    BookingDTO GetBookingByIdAsync(Guid bookingId);
}
