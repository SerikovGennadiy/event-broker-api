using Application.Contracts.Persistance;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repository;

public class BookingRepository : RepositoryBase<Booking>, IBookingRepository
{
    public BookingRepository(AppDbContext context) : base(context)
    { }

    public async Task<Booking?> GetByIdAsync(Guid id) => await Task.FromResult(FindByCondition(x => x.Id == id).SingleOrDefault());
    public async Task<IEnumerable<Booking>> GetAllPendingBookingsAsync() => await FindByCondition(b => b.Status == BookingStatus.Pending).ToListAsync();

    public void CreateBooking(Booking entity) => Create(entity);
}
