namespace Entities.ErrorHandling.Exceptions.Booking;

public class BookingNotFoundException : NotFoundException
{
    public BookingNotFoundException(Guid bookingId) 
        : base($"Booking with ID {bookingId} not found.")
    { }
}
