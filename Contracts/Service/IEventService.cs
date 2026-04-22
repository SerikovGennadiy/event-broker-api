using Shared.DTO;
using Shared.RequestSpecification;

namespace Contracts.Service;

public interface IEventService
{
    // считать данные из хранилища
    IEnumerable<EventDTO> GetAllEvents(EventParameters eventParameters);

    // получить событие по ID
    EventDTO GetEventById(Guid Id);

    // создать событие
    EventDTO CreateEvent(EventDTO eventDTO);

    // обновить данные конкретного события
    void UpdateEvent(Guid eventId, EventDTO eventDTO);

    // событие 
    void DeleteEvent(Guid eventId);

}
