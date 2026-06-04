namespace Entities.ErrorHandling.Exceptions.Booking;

public class BookingNotFoundException : NotFoundException
{
    public BookingNotFoundException(Guid bookingId) 
        : base($"Бронирование с ID {bookingId} отсутствует или удалено.")
    { }
}
