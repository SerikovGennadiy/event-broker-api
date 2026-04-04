using Entities.ErrorHandling.Exceptions;

namespace Entities.ErrorHandling.Exceptions.Event;

public class EventBadDateRangeException : BadRequestException
{
    public EventBadDateRangeException()
        : base("Некорректные даты начала и завершения мероприятия")
    { }
}
