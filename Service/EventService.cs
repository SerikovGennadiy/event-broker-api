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
    public (IEnumerable<EventDTO> eventDTOs, PaginatedResult pageData) GetAllEvents(EventParameters eventParameters)
    {
        if (!eventParameters.ValidatePeriod)
            throw new EventBadDateRangeException();

        var events = repositoryManager.Event.GetAllEvents(eventParameters);
        var eventDTOs = mapper.Map<IEnumerable<EventDTO>>(events);

        return (eventDTOs, pageData: events.PageMetaData);
    }

    public EventDTO GetEventById(Guid eventId)
    {
        var entity = GetEvent(eventId);
        return mapper.Map<EventDTO>(entity);
    }


    public EventDTO CreateEvent(EventDTO eventDTO)
    {
       ValidateEvent(eventDTO);

       var entity = mapper.Map<Event>(eventDTO);   
       entity.Id = Guid.CreateVersion7();

       repositoryManager.Event.CreateEvent(entity);

       return mapper.Map<EventDTO>(entity);
    }

    public void UpdateEvent(Guid eventId, EventDTO eventDTO)
    {
        ValidateEvent(eventDTO);

        var entity = GetEvent(eventId);
        entity = mapper.Map<Event>(eventDTO);
        entity.Id = eventId;

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
