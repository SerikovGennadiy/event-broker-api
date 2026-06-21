using Shared.DTO;
using Shared.RequestSpecification;

namespace Contracts.Service;

public interface IEventService
{
    // считать данные из хранилища
    Task<(IEnumerable<EventInfo> eventDTOs, PaginatedResult pageData)> GetAllEventsAsync(EventParameters eventParameters);

    // получить событие по ID
    Task<EventInfo> GetEventByIdAsync(Guid Id);

    // обновить данные конкретного события
    Task UpdateEventAsync(Guid eventId, EventDTO eventDTO);

    // создать событие
    Task<EventInfo> CreateEventAsync(CreateEvent eventDTO);

    // удалить событие и сввязанные с ним брони 
    Task DeleteEventAsync(Guid eventId);
}
