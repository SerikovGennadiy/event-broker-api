using Entities.Domain.Models;
using Shared.DTO;

namespace Shared.ModelExtensions;

public static class EventExtension
{
    public static EventDTO toDTO(this IReadOnlyEvent @event)
    {
        return new EventDTO(Id: @event.Id,
                            Title: @event.Title,
                            Description: @event.Description,
                            StartAt: @event.StartAt,
                            @event.EndAt);
    }
}
