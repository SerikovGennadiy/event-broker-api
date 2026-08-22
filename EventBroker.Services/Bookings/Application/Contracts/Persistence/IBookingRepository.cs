using Domain.Models;

namespace Application.Contracts.Persistance;

public interface IBookingRepository
{
    Task<IEnumerable<Booking>> GetAllPendingBookingsAsync();
    Task<IEnumerable<Booking>> GetAllBookingsByUserIdAsync(Guid userId);
    Task<Booking?> GetByIdAsync(Guid id);
    void CreateBooking(Booking entity);
}
