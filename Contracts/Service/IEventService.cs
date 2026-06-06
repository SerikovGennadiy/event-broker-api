using Shared.DTO;
using Shared.RequestSpecification;

namespace Contracts.Service;

public interface IEventService
{
    // считать данные из хранилища
    Task<(IEnumerable<EventInfo> eventDTOs, PaginatedResult pageData)> GetAllEventsAsync(EventParameters eventParameters);

    // получить событие по ID
    Task<EventInfo> GetEventByIdAsync(Guid Id);

    // создать событие
    Task<EventInfo> CreateEventAsync(CreateEvent eventDTO);

    // обновить данные конкретного события
    Task UpdateEventAsync(Guid eventId, EventDTO eventDTO);

    // удалить событие и сввязанные с ним брони 
    Task DeleteEventAsync(Guid eventId);
}
