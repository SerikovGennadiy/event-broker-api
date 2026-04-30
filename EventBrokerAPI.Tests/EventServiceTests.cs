using AutoMapper;
using Contracts.Repository;
using Contracts.Service;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Event;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Moq;
using Service;
using Shared.DTO;

namespace EventBrokerAPI.Tests;
public class EventServiceFixture : IDisposable
{
    public EventService EventService { get; }

    public Mock<IRepositoryManager> RepositoryManagerMock { get; } = new();
    public Mock<IEventRepository> EventRepositoryMock { get; } = new();
    public Mock<IMapper> MapperMock { get; } = new();

    public EventServiceFixture()
    {
        RepositoryManagerMock.Setup(x => x.Event)
            .Returns(EventRepositoryMock.Object);

        EventService = new EventService(
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

public class EventServiceTests : IClassFixture<EventServiceFixture>
{
    private readonly EventServiceFixture _fixture;
    public EventServiceTests(EventServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public void CreateEvent_ValidData_ReturnsEvent()
    {
        //Arrange
        var createdEventGuid = Guid.CreateVersion7();
        var eventDTO = new EventDTO(Id: Guid.Empty,
                                    Title: "Event: hiking",
                                    Description: "Info about event",
                                    StartAt: new(2026, 5, 2),
                                    EndAt: new(2026, 5, 3));

        var eventEntity = new Event()
        {
            Id = Guid.Empty,
            Title = "Event: hiking",
            Description = "Info about event",
            StartAt = new(2026, 5, 2),
            EndAt = new(2026, 5, 3)
        };

        var createdEventDTO = eventDTO with { Id = createdEventGuid };

        _fixture.MapperMock.Setup(m => m.Map<Event>(eventDTO)).Returns(eventEntity);
        _fixture.MapperMock.Setup(m => m.Map<EventDTO>(It.IsAny<Event>())).Returns(createdEventDTO);
        _fixture.EventRepositoryMock.Setup(repo => repo.CreateEvent(It.IsAny<Event>())).Verifiable();

        // Act
        var result = _fixture.EventService.CreateEvent(eventDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdEventDTO, result);
        _fixture.RepositoryManagerMock.Verify(rm => rm.Event.CreateEvent(It.IsAny<Event>()), Times.Once());
    }
}
   /// private void ValidateEvent(EventDTO eventDTO)
    //{
    //    if (string.IsNullOrEmpty(eventDTO.Title))
    //        throw new EventNoTitleException();

    //    if (eventDTO.EndAt <= eventDTO.StartAt)
    //        throw new EventBadDateRangeException();
    //}

    //var entity = mapper.Map<Event>(eventDTO);
    //   entity.Id = Guid.CreateVersion7();

    //   repositoryManager.Event.CreateEvent(entity);

    //   return mapper.Map<EventDTO>(entity);