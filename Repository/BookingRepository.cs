using Contracts.Repository;
using Entities.Domain.Models;

namespace Repository;

public class BookingRepository : RepositoryBase<Booking>, IBookingRepository
{
    public BookingRepository(RepositoryContext context): base(context)
    { }

    public IEnumerable<Booking> GetAllBookings() => FindAll();
    public Booking? GetById(Guid id) => FindByCondition(x => x.Id == id).SingleOrDefault();

    public void CreateBooking(Booking entity) => Create(entity);
    public void DeleteBooking(Booking entity) => Delete(entity);
}
