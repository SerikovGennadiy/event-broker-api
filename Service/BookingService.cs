using AutoMapper;
using Contracts.Repository;
using Contracts.Service;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Booking;
using Repository;
using Shared.DTO;
using System.Threading;

namespace Service;

public class BookingService(IRepositoryManager repositoryManager, IMapper mapper) : IBookingService
{
    private static readonly SemaphoreSlim _bookigSemaphore = new(1,1);
    #region Управление уведомлениями
    /// <summary>Отбилось желание забронироваться на мероприятие</summary>
    private static Func<(Guid eventId, int seats), Task>? Rejected;
    internal static void OnRejected(Func<(Guid eventId, int seats), Task> handler) => Rejected ??= handler;

    /// <summary>Выражаем желание забронироваться на мероприятие</summary>
    private static Func<(Guid eventId, int seats), Task>? Booked;
    internal static void OnBooked(Func<(Guid eventId, int seats), Task> handler) => Booked ??= handler;

    internal static void ClearHandlers()
    {
        Booked = null;
        Rejected = null;
    }
    #endregion

    public async Task<BookingDTO> CreateBooking(Guid eventId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        await _bookigSemaphore.WaitAsync(cancellationToken);

        try
        {
            Booking booking;

            Booked?.Invoke((eventId, seats: 1));

            booking = new Booking(Guid.NewGuid(), eventId);
            repositoryManager.Booking.CreateBooking(booking);

            await repositoryManager.SaveAsync();

            return mapper.Map<BookingDTO>(booking);
        }
        finally
        {
            _bookigSemaphore.Release();
        }
    }

    public async Task<BookingDTO> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = await GetBookingAsync(bookingId);
        return mapper.Map<BookingDTO>(booking);
    }

    public async Task<ICollection<BookingDTO>> GetPendingBookingsAsync()
    {
        var bookings = await repositoryManager.Booking.GetAllPendingBookingsAsync();
        var pendingBookingDTOs = mapper.Map<ICollection<BookingDTO>>(bookings);
        return pendingBookingDTOs;
    }

    private async Task<Booking> GetBookingAsync(Guid bookingId)
    {
        var entity = await repositoryManager.Booking.GetByIdAsync(bookingId);
        if (entity is null)
            throw new BookingNotFoundException(bookingId);

        return entity;
    }

    public async Task ConfirmBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        var booking = await GetBookingAsync(bookingId);
        booking.Confirm();

        if(repositoryManager.Booking is BookingRepository repo)
        {
            repo.Update(booking);
        }

        await repositoryManager.SaveAsync();
    }

    public async Task RejectBooingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        var booking = await GetBookingAsync(bookingId);
        booking.Reject();

        Rejected?.Invoke((eventId: booking.EventId, seats: 1));

        if (repositoryManager.Booking is BookingRepository repo)
        {
            repo.Update(booking);
        }

        await repositoryManager.SaveAsync();
    }
}
