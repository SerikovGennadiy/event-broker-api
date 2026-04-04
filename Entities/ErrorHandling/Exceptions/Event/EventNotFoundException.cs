using Entities.ErrorHandling.Exceptions;

namespace Entities.ErrorHandling.Exceptions.Event;

public class EventNotFoundException : NotFoundException
{
    public EventNotFoundException(Guid eventId) 
        : base($"Сущность мероприятия с ID: {eventId} отсутсвует")
    {
    }
}
