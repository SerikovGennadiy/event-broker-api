using AutoMapper;
using Contracts.Repository;
using Contracts.Service;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Booking;
using Repository;
using Shared.DTO;

namespace Service;

public class BookingService(IRepositoryManager repositoryManager, IMapper mapper) : IBookingService
{
    private readonly object _bookingLock = new();

    /// <summary>Отбилось желание забронироваться на мероприятие</summary>
    private static Action<(Guid eventId, int seats)>? Rejected;
    internal static void OnRejected(Action<(Guid eventId, int seats)> handler) => Rejected ??= handler;

    /// <summary>Выражаем желание забронироваться на мероприятие</summary>
    private static Action<(Guid eventId, int seats)>? Booked;
    internal static void OnBooked(Action<(Guid eventId, int seats)> handler) => Booked ??= handler;


    public async Task<BookingDTO> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken)
    {
        // TODO - переделать под ORM, поддерживающую асинхронные операции, чтобы не блокировать поток при работе с БД
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);
        await Task.Yield();

        Booking booking;
        lock (_bookingLock)
        {
            Booked?.Invoke((eventId, seats: 1));

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

    public ICollection<BookingDTO> GetPendingBookings()
    {
        var bookings = repositoryManager.Booking.GetAllPendingBookings();
        var pendingBookingDTOs = mapper.Map<ICollection<BookingDTO>>(bookings);
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

        Rejected?.Invoke((eventId: booking.EventId, seats: 1));

        if (repositoryManager.Booking is BookingRepository repo)
        {
            repo.Update(booking);
        }
    }
}
