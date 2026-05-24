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
        var unexistingGuid = Guid.CreateVersion7();
        List<Event> events = [
            new () { Id = Guid.CreateVersion7(), Title = "A", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(2), TotalSeats = default },
            new () { Id = Guid.CreateVersion7(), Title = "B", StartAt = DateTime.UtcNow.AddDays(2), EndAt = DateTime.UtcNow.AddDays(1), TotalSeats = default },
            new () { Id = Guid.CreateVersion7(), Title = "C", StartAt = DateTime.UtcNow.AddDays(5), EndAt = DateTime.UtcNow.AddDays(1), TotalSeats = default}
        ];

        _fixture.EventRepositoryMock.Setup(r => r.GetById(unexistingGuid)).Returns((Event?)null);

        // Act
        var exception = Record.Exception(() => _fixture.EventService.GetEventById(unexistingGuid));

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
        var dto = new EventDTO("Title", "Description", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), TotalSeats: default);
        _fixture.EventRepositoryMock.Setup(r => r.GetById(unexistingGuid)).Returns((Event?)null);

        // Act & Assert
        Assert.Throws<EventNotFoundException>(() => _fixture.EventService.UpdateEvent(unexistingGuid, dto));
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
                                       TotalSeats: default);
        // Act & Assert
        var expeption = Assert.Throws<EventNoTitleException>(() => _fixture.EventService.CreateEvent(eventDTO));
        Assert.Equal("Отсуствует наименование события", expeption.Message);
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
            TotalSeats = default
        };

        // Arrange
        var updatedEventDTO = new EventDTO(
                                    Title: "Another one super event",
                                    Description: "Info about event",
                                    StartAt: DateTime.UtcNow,
                                    EndAt: DateTime.UtcNow.AddDays(-2),
                                    TotalSeats: default
                                    ); // некорректная дата окончания

        _fixture.EventRepositoryMock.Setup(r => r.GetById(existingGuid)).Returns(existingEvent);

        // Act & Assert
        var exception = Assert.Throws<EventBadDateRangeException>(() => _fixture.EventService.UpdateEvent(existingGuid, updatedEventDTO));
        Assert.Equal("Некорректные даты начала и завершения мероприятия", exception.Message);
    }
}
