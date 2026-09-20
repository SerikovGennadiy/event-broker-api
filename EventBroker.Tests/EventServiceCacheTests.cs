using Events.Application.Common.DTO;
using Moq;
using Xunit;

namespace Events.Tests;

/// <summary>Инвалидация кеша на мутирующих операциях EventService (стратегия "инвалидация при записи").</summary>
public sealed class EventServiceCacheTests(EventsFixture fixture) : IClassFixture<EventsFixture>
{
    [Fact(DisplayName = "Update: кеш event:{id} инвалидируется после сохранения")]
    public async Task UpdateEvent_InvalidatesEventCache()
    {
        // Arrange
        var (service, repos, repo, _, mapper) = fixture.CreateService();
        var entity = EventsFixture.CreateEvent("Старое название", totalSeats: 100, sold: 10);
        repo.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        repo.Setup(r => r.InvalidateEventCacheAsync(entity.Id)).Returns(Task.CompletedTask);
        mapper.Setup(m => m.Map(It.IsAny<EventDTO>(), It.IsAny<Events.Domain.Models.Event>()))
            .Callback((EventDTO dto, Events.Domain.Models.Event e) => e.Title = dto.Title);

        var dto = new EventDTO("Новое название", Description: null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), TotalSeats: 100);

        // Act
        await service.UpdateEventAsync(entity.Id, dto);

        // Assert
        repo.Verify(r => r.InvalidateEventCacheAsync(entity.Id), Times.Once);
        repos.Verify(r => r.SaveAsync(), Times.Once);
        Assert.Equal("Новое название", entity.Title);
    }

    [Fact(DisplayName = "Delete: кеш event:{id} инвалидируется после сохранения")]
    public async Task DeleteEvent_InvalidatesEventCache()
    {
        // Arrange
        var (service, repos, repo, _, _) = fixture.CreateService();
        var entity = EventsFixture.CreateEvent("К удалению", totalSeats: 50, sold: 0);
        repo.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        repo.Setup(r => r.DeleteEvent(entity));
        repo.Setup(r => r.InvalidateEventCacheAsync(entity.Id)).Returns(Task.CompletedTask);

        // Act
        await service.DeleteEventAsync(entity.Id);

        // Assert
        repo.Verify(r => r.DeleteEvent(entity), Times.Once);
        repo.Verify(r => r.InvalidateEventCacheAsync(entity.Id), Times.Once);
        repos.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact(DisplayName = "ReserveSeats (Kafka BookingStarted): кеш инвалидируется")]
    public async Task ReserveSeats_InvalidatesEventCache()
    {
        // Arrange
        var (service, _, repo, outbox, _) = fixture.CreateService();
        var entity = EventsFixture.CreateEvent("Бронируемое", totalSeats: 10, sold: 0);
        repo.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        repo.Setup(r => r.InvalidateEventCacheAsync(entity.Id)).Returns(Task.CompletedTask);

        // Act — путь Kafka-консьюмера BookingStarted -> ReserveSeats.
        await service.ReserveSeats(Guid.NewGuid(), entity.Id, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert: место списано в памяти, кеш инвалидирован, событие саги поставлено в outbox.
        Assert.Equal(9, entity.AvailableSeats);
        repo.Verify(r => r.InvalidateEventCacheAsync(entity.Id), Times.Once);
        outbox.Verify(o => o.EnqueueMessageAsync(It.IsAny<Messaging.Saga.IIntegrationMessage>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "ReleaseSeats (Kafka BookingRejected): кеш инвалидируется")]
    public async Task ReleaseSeats_InvalidatesEventCache()
    {
        // Arrange
        var (service, _, repo, _, _) = fixture.CreateService();
        var entity = EventsFixture.CreateEvent("Освобождаемое", totalSeats: 10, sold: 4);
        repo.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        repo.Setup(r => r.InvalidateEventCacheAsync(entity.Id)).Returns(Task.CompletedTask);

        // Act — путь Kafka-консьюмера BookingRejected -> ReleaseSeats.
        await service.ReleaseSeats(Guid.NewGuid(), entity.Id, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(7, entity.AvailableSeats);
        repo.Verify(r => r.InvalidateEventCacheAsync(entity.Id), Times.Once);
    }

    [Fact(DisplayName = "Create: инвалидация не вызывается (ключа ещё нет, топ на TTL)")]
    public async Task CreateEvent_DoesNotInvalidateCache()
    {
        // Arrange
        var (service, _, repo, _, mapper) = fixture.CreateService();
        repo.Setup(r => r.CreateEvent(It.IsAny<Events.Domain.Models.Event>()));
        repo.Setup(r => r.InvalidateEventCacheAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        var dto = new CreateEvent("Новое", Description: null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), TotalSeats: 20);
        mapper.Setup(m => m.Map<EventInfo>(It.IsAny<Events.Domain.Models.Event>()))
            .Returns((Events.Domain.Models.Event e) => new EventInfo(e.Id, e.Title, e.Description, e.StartAt, e.EndAt, e.TotalSeats, e.AvailableSeats));

        // Act
        var created = await service.CreateEventAsync(dto);

        // Assert
        Assert.Equal("Новое", created.Title);
        repo.Verify(r => r.InvalidateEventCacheAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact(DisplayName = "GetTopSelling: возвращает DTO в порядке репозитория")]
    public async Task GetTopSelling_ReturnsMappedDtos()
    {
        // Arrange
        var (service, _, repo, _, mapper) = fixture.CreateService();
        var high = EventsFixture.CreateEvent("Хит", totalSeats: 100, sold: 90);
        var low = EventsFixture.CreateEvent("Аутсайдер", totalSeats: 100, sold: 10);
        repo.Setup(r => r.GetTop10SellingEventsAsync())
            .ReturnsAsync(new List<Events.Domain.Models.Event> { high, low });
        mapper.Setup(m => m.Map<IEnumerable<EventInfo>>(It.IsAny<IEnumerable<Events.Domain.Models.Event>>()))
            .Returns((IEnumerable<Events.Domain.Models.Event> src) => src
                .Select(e => new EventInfo(e.Id, e.Title, e.Description, e.StartAt, e.EndAt, e.TotalSeats, e.AvailableSeats)).ToList());

        // Act
        var actual = (await service.GetTop10SellingEventsAsync()).ToList();

        // Assert
        Assert.Equal([high.Id, low.Id], actual.Select(d => d.Id));
        Assert.Equal(10, actual[0].AvailableSeats);
    }
}
