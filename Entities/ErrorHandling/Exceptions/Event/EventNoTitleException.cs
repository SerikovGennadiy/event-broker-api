using Entities.ErrorHandling.Exceptions;

namespace Entities.ErrorHandling.Exceptions.Event;
public class EventNoTitleException : BadRequestException
{
    public EventNoTitleException()
        : base("Отсуствует наименование события")
    { }
}
