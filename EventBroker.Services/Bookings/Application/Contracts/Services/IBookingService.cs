using Bookings.Application.Common.DTO;

namespace Bookings.Application.Contracts.Services;

public interface IBookingService
{
    Task<BookingDTO> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<BookingDTO> GetBookingByIdAsync(Guid bookingId);
    Task ConfirmBookingAsync(Guid traceId, Guid bookingId, Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task RejectBooingAsync(Guid traceId, Guid bookingId, Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CancelBookingAsync(Guid traceId, Guid bookingId, Guid userId, CancellationToken cancellationToken = default);
}
