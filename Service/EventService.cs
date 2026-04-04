using Contracts.Service;
using Shared.DTO;

namespace Service;

public class EventService : IEventService
{
    public EventDTO CreateEvent(EventDTO eventDTO)
    {
        throw new NotImplementedException();
    }

    public void DeleteEvent(Guid eventId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<EventDTO> GetAllEvents()
    {
        throw new NotImplementedException();
    }

    public EventDTO GetEventById(Guid Id)
    {
        throw new NotImplementedException();
    }

    public void UpdateEvent(Guid eventId, EventDTO eventDTO)
    {
        throw new NotImplementedException();
    }
}
