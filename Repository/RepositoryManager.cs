using Contracts.Repository;

namespace Repository;

public class RepositoryManager : IRepositoryManager
{
    private IEventRepository Events { get; }
    private IBookingRepository Booking { get; }

    public RepositoryManager(RepositoryContext context)
    {
        Events = new EventRepository(context);
        Booking = new BookingRepository(context);
    }

    public IEventRepository Event => Events;
    public IBookingRepository Booking => Booking;

    // TODO : хранилище локальное
    public void Save() => throw new NotImplementedException();
}
