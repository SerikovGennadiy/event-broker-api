using AutoMapper;
using Contracts.Repository;
using Contracts.Service;

namespace Service;

public class ServiceManager : IServiceManager
{
    private IEventService _eventService;
    private IBookingService _bookingService;

    public ServiceManager(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _eventService = new EventService(repositoryManager, mapper);
        _bookingService = new BookingService(repositoryManager, mapper);
    }
    public IEventService EventService => _eventService;
    public IBookingService BookingService => _bookingService;
}
