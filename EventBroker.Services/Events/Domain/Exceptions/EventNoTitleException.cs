using Events.Domain.Exceptions.Common;

namespace Events.Domain.Exceptions;
public class EventNoTitleException : BadRequestException
{
    public EventNoTitleException()
        : base("Отсуствует наименование события")
    { }
}
