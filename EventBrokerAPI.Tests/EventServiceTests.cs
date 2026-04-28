using AutoMapper;
using Contracts.Repository;
using Moq;
using Repository;
using Service;

namespace EventBrokerAPI.Tests;

public class EventServiceTests
{
    private readonly Mock<IMapper> mockMapper = new();
    private readonly RepositoryContext mockRepositoryContext = new();
    private readonly Mock<IRepositoryManager> mockRepositoryManager = new();

    private readonly EventService eventService;
    private readonly EventRepository eventRepository;

    public EventServiceTests()
    {
        eventRepository = new EventRepository(mockRepositoryContext);
        eventService = new EventService(mockRepositoryManager.Object, mockMapper.Object);
    }
}
