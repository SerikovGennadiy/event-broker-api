using Shared.DTO;
using AutoMapper;
using Contracts.Service;
using Contracts.Repository;
using Repository;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Event;
using Shared.RequestSpecification;

namespace Service;

public class EventService(IRepositoryManager repositoryManager, IMapper mapper) : IEventService
{
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
    }
    #endregion
}
