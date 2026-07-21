namespace Domain.Exceptions.Booking;

public class BookingPastEventException : BadRequestException
{
    public BookingPastEventException(Guid eventId, Guid bookingId)
        : base($"Нельзя создать бронь (ID: {bookingId}) на прошедшее событие (ID: {eventId}).")
    { }

    public BookingPastEventException(Guid eventId)
        : base($"Нельзя создать бронь на прошедшее событие (ID: {eventId}).")
    { }
}