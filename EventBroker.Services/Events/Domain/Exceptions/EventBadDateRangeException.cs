using Events.Domain.Exceptions.Common;

namespace Events.Domain.Exceptions;

public class EventBadDateRangeException : BadRequestException
{
    public EventBadDateRangeException()
        : base("Некорректные даты начала и завершения мероприятия")
    { }
}
