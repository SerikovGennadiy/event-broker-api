using Bookings.Domain.Exceptions.Common;

namespace Bookings.Domain.Exceptions;

public class NoAvailableSeatsException : ConflictException
{
    public NoAvailableSeatsException(Guid eventId)
        : base($"No available seats for this event (id: {eventId}")
    { }
}
