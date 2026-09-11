using Events.Domain.Exceptions.Common;

namespace Events.Domain.Exceptions;

public class EventBadTotalSeatsQuantity : BadRequestException
{
    public EventBadTotalSeatsQuantity()
        : base("Общее количество мест на мероприятии должно быть больше 0")
    { }
}
