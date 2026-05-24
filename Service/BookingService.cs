using Shared.DTO;
using Contracts.Service;
using Contracts.Repository;
using AutoMapper;
using Entities.ErrorHandling.Exceptions.Booking;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Event;
using Repository;

namespace Service;

public class BookingService(IRepositoryManager repositoryManager, IMapper mapper) : IBookingService
{
    private readonly object _bookingLock = new();

    public async Task<BookingDTO> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken)
    {
        // TODO - переделать под ORM, поддерживающую асинхронные операции, чтобы не блокировать поток при работе с БД
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);
        await Task.Yield();

        Booking booking;
        lock (_bookingLock)
        {
            var @event = repositoryManager.Event.GetById(eventId);
            if (@event is null)
                throw new EventNotFoundException(eventId);

            if (!@event.TryReserveSeats())
                throw new NoAvailableSeatsException(eventId);

            booking = new Booking(Guid.NewGuid(), eventId);
            repositoryManager.Booking.CreateBooking(booking);
        }

        return mapper.Map<BookingDTO>(booking);
    }

    public async Task<BookingDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        // TODO - переделать под ORM, поддерживающую асинхронные операции, чтобы не блокировать поток при работе с БД
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);
        await Task.Yield();

        var booking = GetBooking(bookingId);
        return mapper.Map<BookingDTO>(booking);
    }

    public IEnumerable<BookingDTO> GetPendingBookings()
    {
        var bookings = repositoryManager.Booking.GetAllPendingBookings();
        var pendingBookingDTOs = mapper.Map<IEnumerable<BookingDTO>>(bookings);
        return pendingBookingDTOs;
    }

    private Booking GetBooking(Guid bookingId)
    {
        var entity = repositoryManager.Booking.GetById(bookingId);
        if (entity is null)
            throw new BookingNotFoundException(bookingId);

        return entity;
    }

    public void ConfirmBooking(Guid bookingId)
    {
        var booking = GetBooking(bookingId);
        booking.Confirm();

        if(repositoryManager.Booking is BookingRepository repo)
        {
            repo.Update(booking);
        }
    }

    public void RejectBooking(Guid bookingId)
    {
        var booking = GetBooking(bookingId);
        booking.Reject();

        if(repositoryManager.Booking is BookingRepository repo)
        {
            repo.Update(booking);
        }
    }
}
