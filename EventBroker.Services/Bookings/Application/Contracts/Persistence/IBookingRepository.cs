using Bookings.Domain.Models;

namespace Bookings.Application.Contracts.Persistence;

public interface IBookingRepository
{
    Task<IEnumerable<Booking>> GetAllPendingBookingsAsync();
    Task<IEnumerable<Booking>> GetAllBookingsByUserIdAsync(Guid userId);
    Task<IEnumerable<Booking>> GetAllBookingsByEventdAsync(Guid eventId);
    Task<Booking?> GetByIdAsync(Guid id);
    void CreateBooking(Booking entity);
}
