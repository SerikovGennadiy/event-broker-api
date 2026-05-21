using Entities.Domain.Models;
using Shared.DTO;

namespace Shared.ModelExtensions;

public static class EventExtension
{
    public static EventInfo toDTO(this IReadOnlyEvent @event)
    {
        return new EventInfo(Id: @event.Id,
                             Title: @event.Title,
                             Description: @event.Description,
                             StartAt: @event.StartAt,
                             EndA: @event.EndAt,
                             TotalSeats: @event.TotalSeats,
                             AvailableSeats: @event.AvailableSeats);
    }
}
