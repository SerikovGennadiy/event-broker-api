using Application.Common.DTO;
using Application.Common.RequestSpecification;

namespace Application.Contracts.Services;

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

    // зарезервировать место
    Task ReserveSeats((Guid eventId, int seats) callFromBooking);

    // отказаться от брони
    Task ReleaseSeats((Guid eventId, int seats) recallFromBooking);
}
