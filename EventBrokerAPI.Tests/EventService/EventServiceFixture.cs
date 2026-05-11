using AutoMapper;
using Contracts.Repository;
using Moq;
using EventServiceType = Service.EventService;

namespace EventBrokerAPI.Tests.Fixture.EventService;
public class EventServiceFixture : IDisposable
{
    public EventServiceType EventService { get; }

    public Mock<IRepositoryManager> RepositoryManagerMock { get; } = new();
    public Mock<IEventRepository> EventRepositoryMock { get; } = new();
    public Mock<IMapper> MapperMock { get; } = new();

    public EventServiceFixture()
    {
        RepositoryManagerMock.Setup(x => x.Event)
            .Returns(EventRepositoryMock.Object);

        EventService = new EventServiceType(
            RepositoryManagerMock.Object,
            MapperMock.Object
        );
    }

    public void Dispose()
    {
        RepositoryManagerMock.Reset();
        EventRepositoryMock.Reset();
        MapperMock.Reset();

        GC.SuppressFinalize(this);
    }
}