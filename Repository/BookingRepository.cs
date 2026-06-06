using Contracts.Repository;
using Entities.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class BookingRepository : RepositoryBase<Booking>, IBookingRepository
{
    public BookingRepository(AppDbContext context) : base(context)
    { }

    public async Task<Booking?> GetByIdAsync(Guid id) => await FindByCondition(x => x.Id == id).SingleOrDefaultAsync();
    public async Task<IEnumerable<Booking>> GetAllPendingBookingsAsync() => await FindByCondition(b => b.Status == BookingStatus.Pending).ToListAsync();
    
    public void CreateBooking(Booking entity) => Create(entity);
}
