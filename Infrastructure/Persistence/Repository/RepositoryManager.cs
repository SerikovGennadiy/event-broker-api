using Application.Contracts.Persistance;

namespace Infrastructure.Persistence.Repository;

public class RepositoryManager(AppDbContext context) : IRepositoryManager
{
    private IEventRepository Events { get; } = new EventRepository(context);
    private IBookingRepository Bookings { get; } = new BookingRepository(context);
    private IUserRepository Users { get; } = new UserRepository(context);

    public IEventRepository Event => Events;
    public IBookingRepository Booking => Bookings;
    public IUserRepository User => Users;

    public async Task SaveAsync() => await context.SaveChangesAsync();
}
