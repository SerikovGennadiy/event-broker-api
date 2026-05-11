using Contracts.Repository;

namespace Repository;

public class RepositoryManager : IRepositoryManager
{
    private IEventRepository Events { get; }
    private IBookingRepository Bookings { get; }

    public RepositoryManager(RepositoryContext context)
    {
        Events = new EventRepository(context);
        Bookings = new BookingRepository(context);
    }

    public IEventRepository Event => Events;
    public IBookingRepository Booking => Bookings;
    // TODO : хранилище локальное
    public void Save() => throw new NotImplementedException();
}
