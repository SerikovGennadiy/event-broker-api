using AutoMapper;
using Contracts.Repository;
using Moq;
using BookingServiceType = Service.BookingService;

namespace EventBrokerAPI.Tests.BookingService;
public class BookingServiceFixture : IDisposable
{
    public BookingServiceType BookingService { get; }

    public Mock<IRepositoryManager> RepositoryManagerMock { get; } = new();
    public Mock<IEventRepository> EventRepositoryMock { get; } = new();
    public Mock<IBookingRepository> BookingRepositoryMock { get; } = new();
    public Mock<IMapper> MapperMock { get; } = new();

    public BookingServiceFixture()
    {
        RepositoryManagerMock.Setup(x => x.Event)
            .Returns(EventRepositoryMock.Object);

        RepositoryManagerMock.Setup(x => x.Booking)
            .Returns(BookingRepositoryMock.Object);

        BookingService = new BookingServiceType(
            RepositoryManagerMock.Object,
            MapperMock.Object
        );
    }

    public void Dispose()
    {
        RepositoryManagerMock.Reset();
        EventRepositoryMock.Reset();
        BookingRepositoryMock.Reset();
        MapperMock.Reset();

        GC.SuppressFinalize(this);
    }
}
