using Shared.DTO;
using Contracts.Service;
using Contracts.Repository;
using AutoMapper;
using Entities.ErrorHandling.Exceptions.Booking;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Event;

namespace Service;

public class BookingService(IRepositoryManager repositoryManager, IMapper mapper) : IBookingService
{
    public BookingDTO CreateBookingAsync(Guid eventId)
    {
        ValidateBookingFor(eventId);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now
        };

        repositoryManager.Booking.CreateBooking(booking);

        return mapper.Map<BookingDTO>(booking);
    }

    public BookingDTO GetBookingByIdAsync(Guid bookingId)
    {
        var booking = GetBooking(bookingId);
        return mapper.Map<BookingDTO>(booking);
    }

    private Booking GetBooking(Guid bookingId)
    {
        var entity = repositoryManager.Booking.GetById(bookingId);
        if (entity is null)
            throw new BookingNotFoundException(bookingId);

        return entity;
    }

    private void ValidateBookingFor(Guid eventId)
    {
       var @event = repositoryManager.Event.GetById(eventId);
         if (@event is null)
                throw new EventNotFoundException(eventId);
    }
}
