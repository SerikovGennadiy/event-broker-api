namespace Entities.ErrorHandling.Exceptions.Booking;

public class NoAvailableSeatsException : ConflictException
{
    public NoAvailableSeatsException(Guid eventId)
        : base($"No available seats for this event (id: {eventId}")
    { }
}
