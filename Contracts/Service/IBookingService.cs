using Shared.DTO;

namespace Contracts.Service;

public interface IBookingService
{
    BookingDTO CreateBookingAsync(Guid eventId);
    BookingDTO GetBookingByIdAsync(Guid bookingId);
}
