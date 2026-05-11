using Entities.Domain.Models;

namespace Contracts.Repository;

public interface IBookingRepository
{
    IEnumerable<Booking> GetAllPendingBookings();

    Booking? GetById(Guid id); 
    void CreateBooking(Booking entity); 
}
