using Shared.DTO;

namespace Contracts.Service;

public interface IEventService
{
    // создать событие
    EventDTO CreateEvent(EventDTO eventDTO);

    // считать данные из хранилища
    IEnumerable<EventDTO> GetAllEvents();

    // получить событие по ID
    EventDTO GetEventById(Guid Id);
  
    // обновить данные конкретного события
    void UpdateEvent(Guid eventId, EventDTO eventDTO );

    // событие 
    void DeleteEvent(Guid eventId);

}
