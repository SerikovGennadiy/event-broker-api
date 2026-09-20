
using AutoMapper;
using Events.Application.Common.DTO;
using Events.Application.Common.RequestSpecification;
using Events.Application.Contracts.Persistence;
using Events.Application.Contracts.Services;
using Events.Application.Contracts.Services.Messaging;
using Events.Domain.Exceptions;
using Events.Domain.Models;
using Messaging;
using Messaging.Events;

namespace Events.Application.Services;

public class EventService(IRepositoryManager repositoryManager, IMapper mapper, IOutboxService outboxService) : IEventService
{
    public async Task<(IEnumerable<EventInfo> eventDTOs, PaginatedResult pageData)> GetAllEventsAsync(EventParameters eventParameters)
    {
        if (!eventParameters.IsDateRangeValid)
            throw new EventBadDateRangeException();

        var events = await repositoryManager.Event.GetAllEventsAsync(eventParameters);
        var eventDTOs = mapper.Map<IEnumerable<EventInfo>>(events);

        return (eventDTOs, pageData: events.PageMetaData);
    }

    public async Task<EventInfo> GetEventByIdAsync(Guid eventId)
    {
        var entity = await GetEvent(eventId);
        return mapper.Map<EventInfo>(entity);
    }

    public async Task<IEnumerable<EventInfo>> GetTop10SellingEventsAsync()
    {
        var events = await repositoryManager.Event.GetTop10SellingEventsAsync();
        return mapper.Map<IEnumerable<EventInfo>>(events);
    }

    public async Task<EventInfo> CreateEventAsync(CreateEvent eventDTO, CancellationToken stoppingToken = default)
    {
        ValidateEvent(eventDTO);

        var entity = Event.Create(eventDTO.Title, eventDTO.StartAt, eventDTO.EndAt, eventDTO.Description, eventDTO.TotalSeats);
        repositoryManager.Event.CreateEvent(entity);

        var integrationEvent = new EventCreatedOrUpdated(TraceId: Guid.CreateVersion7(),
                                                         EventId: entity.Id,
                                                         Title: entity.Title,
                                                         StartAt: entity.StartAt,
                                                         EndAt: entity.EndAt);
        await repositoryManager.SaveAsync();

        return mapper.Map<EventInfo>(entity);
    }

    public async Task UpdateEventAsync(Guid eventId, EventDTO eventDTO, CancellationToken stoppingToken = default)
    {
        ValidateEvent(eventDTO);

        var entity = await GetEvent(eventId);

        mapper.Map(eventDTO, entity);

        var integrationEvent = new EventCreatedOrUpdated(TraceId: Guid.CreateVersion7(),
                                                         EventId: entity.Id,
                                                         Title: entity.Title,
                                                         StartAt: entity.StartAt,
                                                         EndAt: entity.EndAt);
        await outboxService.EnqueueMessageAsync(integrationEvent, Topics.EventIntegration, stoppingToken);

        await repositoryManager.SaveAsync();
        await repositoryManager.Event.InvalidateEventCacheAsync(entity.Id);
    }

    public async Task DeleteEventAsync(Guid eventId, CancellationToken stoppingToken = default)
    {
        var entity = await GetEvent(eventId);
        repositoryManager.Event.DeleteEvent(entity);

        var integrationEvent = new EventDeleted(TraceId: Guid.CreateVersion7(), EventId: entity.Id);
        await outboxService.EnqueueMessageAsync(integrationEvent, Topics.EventIntegration, stoppingToken);

        await repositoryManager.SaveAsync();
        await repositoryManager.Event.InvalidateEventCacheAsync(entity.Id);
    }

    public async Task ReserveSeats(Guid traceId, Guid eventId, Guid bookingId, Guid userId, CancellationToken stoppingToken)
    {
        var @event = await GetEvent(eventId);
        if (@event.TryReserveSeats(count: 1))       
            await outboxService.EnqueueMessageAsync(@event: new SeatReserved(TraceId: traceId,
                                                                             BookingId: bookingId,
                                                                             EventId: eventId,
                                                                             UserId: userId),
                                                    topic: Topics.BookingProcessing,
                                                    cancellationToken: stoppingToken);        
        else
            await outboxService.EnqueueMessageAsync(@event: new SeatReservationFailed(TraceId: traceId,
                                                                                      BookingId: bookingId,
                                                                                      EventId: eventId,
                                                                                      UserId: userId,
                                                                                      Error: "Нет доступного кол-ва мест"),
                                                     topic: Topics.BookingProcessing,
                                                     cancellationToken: stoppingToken);


        await repositoryManager.SaveAsync();
        await repositoryManager.Event.InvalidateEventCacheAsync(eventId);
    }

    public async Task ReleaseSeats(Guid traceId, Guid eventId, Guid bookingId, Guid userId,  CancellationToken stoppingToken)
    {
        var @event = await GetEvent(eventId);
        @event.ReleaseSeats(count: 1);

        await outboxService.EnqueueMessageAsync(@event: new SeatReleased(TraceId: traceId,
                                                                         BookingId: bookingId,
                                                                         EventId: eventId,
                                                                         UserId: userId),
                                                 topic: Topics.BookingProcessing,
                                                 cancellationToken: stoppingToken);

        await repositoryManager.SaveAsync();
        await repositoryManager.Event.InvalidateEventCacheAsync(eventId);
    }

    #region Обертки с валидацей 
    private async Task<Event> GetEvent(Guid eventId)
    {
        var entity = await repositoryManager.Event.GetByIdAsync(eventId);
        if (entity == null)
            throw new EventNotFoundException(eventId);

        return entity;
    }

    private void ValidateEvent(EventDTO eventDTO)
    {
        if (string.IsNullOrEmpty(eventDTO.Title))
            throw new EventNoTitleException();

        if (eventDTO.EndAt <= eventDTO.StartAt)
            throw new EventBadDateRangeException();

        if (eventDTO.TotalSeats <= 0)
            throw new EventBadTotalSeatsQuantity();
    }
    #endregion

}
