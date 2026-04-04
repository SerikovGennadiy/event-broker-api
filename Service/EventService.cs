using Shared.DTO;
using AutoMapper;
using Entities.Models;
using Contracts.Service;
using Contracts.Repository;
using Repository;

namespace Service;

public class EventService(IRepositoryManager repositoryManager, IMapper mapper) : IEventService
{
    public EventDTO CreateEvent(EventDTO eventDTO)
    {
       var entity = mapper.Map<Event>(eventDTO);     
           entity.Id = Guid.CreateVersion7();

       repositoryManager.Event.CreateEvent(entity);

       return mapper.Map<EventDTO>(entity);
    }

    public void DeleteEvent(Guid eventId)
    {
        var entity = GetEvent(eventId);
        repositoryManager.Event.DeleteEvent(entity);
    }

    public IEnumerable<EventDTO> GetAllEvents()
    {
        var entitiesDTO = repositoryManager.Event.GetAllEvents();
        return mapper.Map<IEnumerable<EventDTO>>(entitiesDTO);
    }

    public EventDTO GetEventById(Guid eventId)
    {
        var entity = GetEvent(eventId);
        return mapper.Map<EventDTO>(entity);
    }

    public void UpdateEvent(Guid eventId, EventDTO eventDTO)
    {
        var entity = GetEvent(eventId);
        if(repositoryManager.Event is EventRepository repo)
        {
            repo.Update(entity);
        }
    }

    private Event GetEvent(Guid eventId)
    {
        var entity = repositoryManager.Event.GetById(eventId);
        if (entity == null)
            throw new Exception("");

        return entity;
    }
}
