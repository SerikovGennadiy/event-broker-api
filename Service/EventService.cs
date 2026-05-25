using AutoMapper;
using Contracts.Repository;
using Contracts.Service;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Booking;
using Entities.ErrorHandling.Exceptions.Event;
using Repository;
using Shared.DTO;
using Shared.RequestSpecification;
using System.ComponentModel.DataAnnotations;

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

        // TODO перевести в статический конструктор потом, пока контроль в порядка контрактов сервисов в DI
        BookingService.OnBooked(ReserveSeats);
        BookingService.OnRejected(ReleaseSeats);
    }

    public (IEnumerable<EventInfo> eventDTOs, PaginatedResult pageData) GetAllEvents(EventParameters eventParameters)
    {
        // TODO этот инвариант должен сидеть в отдельном классе валидаторе, который будет использоваться в контроллере, а не в сервисе?
        if (!eventParameters.IsDateRangeValid)
            throw new EventBadDateRangeException();

        var events = repositoryManager.Event.GetAllEvents(eventParameters);
        var eventDTOs = mapper.Map<IEnumerable<EventInfo>>(events);

        return (eventDTOs, pageData: events.PageMetaData);
    }

    public EventInfo GetEventById(Guid eventId)
    {
        var entity = GetEvent(eventId);
        return mapper.Map<EventInfo>(entity);
    }

    public EventInfo CreateEvent(CreateEvent eventDTO)
    {
       ValidateEvent(eventDTO);

       var entity = Event.Create(eventDTO.Title, eventDTO.StartAt, eventDTO.EndAt, eventDTO.Description, eventDTO.TotalSeats);
       repositoryManager.Event.CreateEvent(entity);

       return mapper.Map<EventInfo>(entity);
    }

    public void UpdateEvent(Guid eventId, EventDTO eventDTO)
    {
        ValidateEvent(eventDTO);

        var entity = GetEvent(eventId);

        // обновление только измененных полей маппером
        entity = mapper.Map<Event>(eventDTO);
        // пока что EventDTO общий
        if(repositoryManager.Event is EventRepository repo)
        {
            repo.Update(entity);
        }
    }

    public void DeleteEvent(Guid eventId)
    {
        var entity = GetEvent(eventId);
        repositoryManager.Event.DeleteEvent(entity);
    }

    #region Обертки с валидацей 
    private Event GetEvent(Guid eventId)
    {
        var entity = repositoryManager.Event.GetById(eventId);
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

    private void ReserveSeats((Guid eventId, int seats) callFromBooking)
    {
        var @event = GetEvent(callFromBooking.eventId);
        if (!@event.TryReserveSeats(callFromBooking.seats))
            throw new NoAvailableSeatsException(callFromBooking.eventId);
    }

    private void ReleaseSeats((Guid eventId, int seats) recallFromBooking)
    {
        var @event = GetEvent(recallFromBooking.eventId);
        @event.ReleaseSeats(recallFromBooking.seats);
    }
    #endregion
}
