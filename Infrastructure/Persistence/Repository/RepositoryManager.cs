namespace Infrastructure.Persistence.Repository;

public class RepositoryManager(AppDbContext context) : IRepositoryManager
{
    private IEventRepository Events { get; } = new EventRepository(context);
    private IBookingRepository Bookings { get; } = new BookingRepository(context);

    public IEventRepository Event => Events;
    public IBookingRepository Booking => Bookings;

    public async Task SaveAsync() => await context.SaveChangesAsync();
}
