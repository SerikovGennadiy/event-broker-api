using Events.Application.Common.DTO;
using Events.Domain.Models;

namespace Events.Application.Common.Extensions;
/// <summary> Для генерации DTO объекта из доменной суности </summary>
public static class EventExtension
{
    public static EventInfo toDTO(this IReadOnlyEvent @event)
    {
        return new EventInfo(Id: @event.Id,
                             Title: @event.Title,
                             Description: @event.Description,
                             StartAt: @event.StartAt,
                             EndAt: @event.EndAt,
                             TotalSeats: @event.TotalSeats,
                             AvailableSeats: @event.AvailableSeats);
    }
}
