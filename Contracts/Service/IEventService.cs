using Shared.DTO;
using Shared.RequestSpecification;

namespace Contracts.Service;

public interface IEventService
{
    // считать данные из хранилища
    (IEnumerable<EventInfo> eventDTOs, PaginatedResult pageData) GetAllEvents(EventParameters eventParameters);

    // получить событие по ID
    EventInfo GetEventById(Guid Id);

    // создать событие
    EventInfo CreateEvent(CreateEvent eventDTO);

    // обновить данные конкретного события
    void UpdateEvent(Guid eventId, EventDTO eventDTO);

    // событие 
    void DeleteEvent(Guid eventId);

}
