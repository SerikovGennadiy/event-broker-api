using AutoMapper;

namespace Service;

public class EventService : IEventService
{
    private readonly IRepositoryManager repositoryManager;
    private readonly IMapper mapper;

    public EventService(IRepositoryManager _repositoryManager,
                        IMapper _mapper)
    {
        repositoryManager = _repositoryManager;
        mapper = _mapper;
    }

    public async Task<(IEnumerable<EventInfo> eventDTOs, PaginatedResult pageData)> GetAllEventsAsync(EventParameters eventParameters)
    {
        // TODO этот инвариант должен сидеть в отдельном классе валидаторе, который будет использоваться в контроллере, а не в сервисе?
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

    public async Task<EventInfo> CreateEventAsync(CreateEvent eventDTO)
    {
        ValidateEvent(eventDTO);

        var entity = Event.Create(eventDTO.Title, eventDTO.StartAt, eventDTO.EndAt, eventDTO.Description, eventDTO.TotalSeats);
        repositoryManager.Event.CreateEvent(entity);

        await repositoryManager.SaveAsync();

        return mapper.Map<EventInfo>(entity);
    }

    public async Task UpdateEventAsync(Guid eventId, EventDTO eventDTO)
    {
        ValidateEvent(eventDTO);

        var entity = await GetEvent(eventId);

        mapper.Map(eventDTO, entity);

        await repositoryManager.SaveAsync();
    }

    public async Task DeleteEventAsync(Guid eventId)
    {
        var entity = await GetEvent(eventId);
        repositoryManager.Event.DeleteEvent(entity);
        await repositoryManager.SaveAsync();
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

    public async Task ReserveSeats((Guid eventId, int seats) callFromBooking)
    {
        var @event = await GetEvent(callFromBooking.eventId);
        if (!@event.TryReserveSeats(callFromBooking.seats))
            throw new NoAvailableSeatsException(callFromBooking.eventId);

        await repositoryManager.SaveAsync();
    }

    public async Task ReleaseSeats((Guid eventId, int seats) recallFromBooking)
    {
        var @event = await GetEvent(recallFromBooking.eventId);
        @event.ReleaseSeats(recallFromBooking.seats);

        await repositoryManager.SaveAsync();
    }
    #endregion
}
