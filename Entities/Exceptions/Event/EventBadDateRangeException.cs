namespace Entities.Exceptions.Event;

public class EventBadDateRangeException : BadRequestException
{
    public EventBadDateRangeException()
        : base("Некорректные даты начала и завершения мероприятия")
    { }
}
