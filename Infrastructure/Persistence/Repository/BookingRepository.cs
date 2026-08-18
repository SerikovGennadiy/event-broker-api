using Application.Contracts.Persistance;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repository;

public class BookingRepository : RepositoryBase<Booking>, IBookingRepository
{
    public BookingRepository(AppDbContext context) : base(context)
    { }

    public async Task<IEnumerable<Booking>> GetAllPendingBookingsAsync() => await FindByCondition(b => b.Status == BookingStatus.Pending).ToListAsync();
    public async Task<IEnumerable<Booking>> GetAllBookingsByUserIdAsync(Guid userId) =>
        await FindByCondition(b => b.UserId == userId && b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Rejected).ToListAsync();
    public async Task<Booking?> GetByIdAsync(Guid id) => await Task.FromResult(FindByCondition(x => x.Id == id).SingleOrDefault());

    public void CreateBooking(Booking entity) => Create(entity);
}
