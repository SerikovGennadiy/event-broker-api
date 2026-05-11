using AutoMapper;
using Contracts.Repository;
using Moq;
using BookingServiceType = Service.BookingService;

namespace EventBrokerAPI.Tests.BookingService;
internal class BookingServiceFixture : IDisposable
{
    public BookingServiceType EventService { get; }

    public Mock<IRepositoryManager> RepositoryManagerMock { get; } = new();
    public Mock<IBookingRepository> BookingRepositoryMock { get; } = new();
    public Mock<IMapper> MapperMock { get; } = new();

    public BookingServiceFixture()
    {
        RepositoryManagerMock.Setup(x => x.Booking)
            .Returns(BookingRepositoryMock.Object);

        EventService = new BookingServiceType(
            RepositoryManagerMock.Object,
            MapperMock.Object
        );
    }

    public void Dispose()
    {
        RepositoryManagerMock.Reset();
        BookingRepositoryMock.Reset();
        MapperMock.Reset();

        GC.SuppressFinalize(this);
    }
}
