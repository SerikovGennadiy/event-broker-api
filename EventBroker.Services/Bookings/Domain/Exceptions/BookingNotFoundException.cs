using Bookings.Domain.Exceptions.Common;

namespace Bookings.Domain.Exceptions;

public class BookingNotFoundException : NotFoundException
{
    public BookingNotFoundException(Guid bookingId)
        : base($"Бронирование с ID {bookingId} отсутствует или удалено.")
    { }
}
