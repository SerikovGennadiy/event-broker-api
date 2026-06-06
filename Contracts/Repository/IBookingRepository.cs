using Entities.Domain.Models;

namespace Contracts.Repository;

public interface IBookingRepository
{
    Task<IEnumerable<Booking>> GetAllPendingBookingsAsync();
    Task<Booking?> GetByIdAsync(Guid id); 
    void CreateBooking(Booking entity); 
}
