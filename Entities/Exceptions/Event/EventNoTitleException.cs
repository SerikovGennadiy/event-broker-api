namespace Entities.Exceptions.Event;
public class EventNoTitleException : BadRequestException
{
    public EventNoTitleException()
        : base("Отсуствует наименование события")
    { }
}
