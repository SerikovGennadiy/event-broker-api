using Events.Application.Common.DTO;
using Events.Application.Common.RequestSpecification;

namespace Events.Application.Contracts.Services;

public interface IEventService
{
    /// <summary>Cчитать данные из хранилища</summary>
    Task<(IEnumerable<EventInfo> eventDTOs, PaginatedResult pageData)> GetAllEventsAsync(EventParameters eventParameters);

    /// <summary>Получить событие по идентификатору</summary>
    Task<EventInfo> GetEventByIdAsync(Guid Id);

    #region Старты саг IEventIntegration
    /// <summary>Создать событие</summary>
    /// <remarks>СТАРТ САГИ: EventCreatedOrUpdated: IEventIntegration</remarks>
    Task<EventInfo> CreateEventAsync(CreateEvent eventDTO, CancellationToken stoppingToken);

    /// <summary>Обновить данные конкретного события</summary>
    /// <remarks>СТАРТ САГИ: EventCreatedOrUpdated: IEventInegration</remarks>
    Task UpdateEventAsync(Guid eventId, EventDTO eventDTO, CancellationToken stoppingToken = default);

    /// <summary>Удалить событие и сввязанные с ним брони </summary>
    /// <remarks>СТАРТ САГИ: EventDeleted: IEventIntegration</remarks>
    Task DeleteEventAsync(Guid eventId, CancellationToken stoppingToken = default);
    #endregion

    #region Шаги саг IBookingProcessing
    /// <summary>Зарезервировать места на мероприятие</summary>
    /// <remarks>сага BookingCreated : IBookingProcessing. запуск см в <see cref="Events.Application.Background.Consumer"/></remarks>
    Task ReserveSeats(Guid traceId, Guid eventId, Guid bookingId, Guid userId, CancellationToken cancellationToken);

    /// <summary>Освободить забронированные места на мероприятие</summary>
    /// <remarks>сага BookingCancelled : IBookingProcessingзапуск см в <see cref="Events.Application.Background.Consumer"/></remarks>
    Task ReleaseSeats(Guid traceId, Guid eventId, Guid bookingId, Guid userId, CancellationToken cancellationToken);
    #endregion
}
