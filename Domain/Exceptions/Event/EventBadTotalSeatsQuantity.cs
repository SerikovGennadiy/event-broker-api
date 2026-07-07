using Domain.Exceptions;

namespace Domain.Exceptions.Event;

public class EventBadTotalSeatsQuantity : BadRequestException
{
    public EventBadTotalSeatsQuantity()
        : base("Общее количество мест на мероприятии должно быть больше 0")
    {  }
}
