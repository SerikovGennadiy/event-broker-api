using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Event;
using EventBrokerAPI.Tests.Fixture.EventService;
using Shared.DTO;

namespace EventBrokerAPI.Tests.EventService.Exceptions;

public class Tests : IClassFixture<EventServiceFixture>
{
    private readonly EventServiceFixture _fixture;
    public Tests(EventServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [Trait("Event", "Exceptions")]
    public void GetEventById_NotExistId_ThrowsEventNotFoundException()
    {
        // Arrange
        var unexistingGuid = Guid.NewGuid();
        List<Event> events = [
            new () { Id = Guid.NewGuid(), Title = "A", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(2), TotalSeats = default },
            new () { Id = Guid.NewGuid(), Title = "B", StartAt = DateTime.UtcNow.AddDays(2), EndAt = DateTime.UtcNow.AddDays(1), TotalSeats = default },
            new () { Id = Guid.NewGuid(), Title = "C", StartAt = DateTime.UtcNow.AddDays(5), EndAt = DateTime.UtcNow.AddDays(1), TotalSeats = default}
        ];

        _fixture.EventRepositoryMock.Setup(r => r.GetByIdAsync(unexistingGuid)).Returns((Event?)null);

        // Act
        var exception = Record.Exception(() => _fixture.EventService.GetEventByIdAsync(unexistingGuid));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<EventNotFoundException>(exception);
    }

    [Fact]
    [Trait("Event", "Exceptions")]
    public void UpdateEvent_NotExistId_ThrowsEventNotFoundException()
    {
        // Arrange
        var unexistingGuid = Guid.NewGuid();
        var dto = new EventDTO("Title", "Description", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), TotalSeats: 10);
        _fixture.EventRepositoryMock.Setup(r => r.GetByIdAsync(unexistingGuid)).Returns((Event?)null);

        // Act & Assert
        Assert.Throws<EventNotFoundException>(() => _fixture.EventService.UpdateEventAsync(unexistingGuid, dto));
    }

    [Fact]
    [Trait("Event", "Exceptions")]
    public void CreateEvent_IncorrectTitle_ThrowsEventNoTitleException()
    {
        // Arrange
        var eventDTO = new CreateEvent(Title: string.Empty, // некорректный заголовок
                                       Description: "Info about event",
                                       StartAt: DateTime.UtcNow,
                                       EndAt: DateTime.UtcNow.AddDays(1),
                                       TotalSeats: 10);
        // Act & Assert
        var expeption = Assert.Throws<EventNoTitleException>(() => _fixture.EventService.CreateEventAsync(eventDTO));
        Assert.Equal("Отсуствует наименование события", expeption.Message);
    }

    [Fact]
    [Trait("Event", "Exceptions")]
    public void CreateEvent_IncorrectTotalSeats_ThrowsEventBadTotalSeatsQuantity()
    {
        // Arrange
        var eventDTO = new CreateEvent(Title: "Event without seats", // некорректный заголовок
                                       Description: "Info about event",
                                       StartAt: DateTime.UtcNow,
                                       EndAt: DateTime.UtcNow.AddDays(1),
                                       TotalSeats: 0);
        // Act & Assert
        var expeption = Assert.Throws<EventBadTotalSeatsQuantity>(() => _fixture.EventService.CreateEventAsync(eventDTO));
        Assert.Equal("Общее количество мест на мероприятии должно быть больше 0", expeption.Message);
    }


    [Fact]
    [Trait("Event", "Exceptions")]
    public void UpdateEvent_IncorrectDateRange_ThrowsEventBadDateRangeException()
    {
        // Arrange
        var existingGuid = Guid.NewGuid();
        var existingEvent = new Event
        {
            Id = existingGuid,
            Title = "Existing Event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = 10
        };

        // Arrange
        var updatedEventDTO = new EventDTO(
                                    Title: "Another one super event",
                                    Description: "Info about event",
                                    StartAt: DateTime.UtcNow,
                                    EndAt: DateTime.UtcNow.AddDays(-2),
                                    TotalSeats: 10
                                    ); // некорректная дата окончания

        _fixture.EventRepositoryMock.Setup(r => r.GetByIdAsync(existingGuid)).Returns(existingEvent);

        // Act & Assert
        var exception = Assert.Throws<EventBadDateRangeException>(() => _fixture.EventService.UpdateEventAsync(existingGuid, updatedEventDTO));
        Assert.Equal("Некорректные даты начала и завершения мероприятия", exception.Message);
    }
}
