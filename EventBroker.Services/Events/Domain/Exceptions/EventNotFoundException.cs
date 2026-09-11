using Events.Domain.Exceptions.Common;

namespace Events.Domain.Exceptions;

public class EventNotFoundException : NotFoundException
{
    public EventNotFoundException(Guid eventId)
        : base($"Сущность мероприятия с ID: {eventId} отсутсвует")
    {
    }
}
