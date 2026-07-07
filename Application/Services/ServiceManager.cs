using Application.Contracts.Persistance;
using Application.Contracts.Services;
using AutoMapper;

namespace Application.Services;

public class ServiceManager : IServiceManager
{
    private IEventService _eventService;
    private IBookingService _bookingService;

    public ServiceManager(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _eventService = new EventService(repositoryManager, mapper);
        _bookingService = new BookingService(repositoryManager, _eventService, mapper);
    }
    public IEventService EventService => _eventService;
    public IBookingService BookingService => _bookingService;
}
