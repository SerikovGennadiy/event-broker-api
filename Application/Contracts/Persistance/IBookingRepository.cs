using Entities.Domain.Models;

namespace Application.Contracts.Persistance;

public interface IBookingRepository
{
    Task<IEnumerable<Booking>> GetAllPendingBookingsAsync();
    Task<Booking?> GetByIdAsync(Guid id); 
    void CreateBooking(Booking entity); 
}
