using Entities.Domain.Models;

namespace Contracts.Repository;

public interface IBookingRepository
{
    IEnumerable<Booking> GetAllBookings(); 
    Booking? GetById(Guid id); 

    void CreateBooking(Booking entity); 
    void DeleteBooking(Booking entity);
}
