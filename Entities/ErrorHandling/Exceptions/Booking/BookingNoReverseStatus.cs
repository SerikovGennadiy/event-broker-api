namespace Entities.ErrorHandling.Exceptions.Booking;
public class BookingNoReverseStatus : BadRequestException
{
    public BookingNoReverseStatus(Guid eventId, Guid bookingId, string message) 
        : base($"Бронь (ID: {bookingId}) мероприятия (ID: {eventId})  и бронирование. {message}")
    { }
}
