namespace Domain.Exceptions.Booking;
public class BookingNoReverseStatus : BadRequestException
{
    public BookingNoReverseStatus(Guid eventId, Guid bookingId, string message)
        : base($"Ошибка смены статуса на предыдущий (ID: {bookingId}) мероприятия (ID: {eventId})  и бронирование. {message}")
    { }
}
