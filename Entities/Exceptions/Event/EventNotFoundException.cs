namespace Entities.Exceptions.Event;

public class EventNotFoundException : NotFoundException
{
    public EventNotFoundException(Guid eventId) 
        : base($"Сущность мероприятия с ID: {eventId} отсутсвует")
    {
    }
}
