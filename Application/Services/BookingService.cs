using Application.Common.DTO;
using Application.Contracts.Persistance;
using Application.Contracts.Services;
using Application.Contracts.Services.Auth;
using AutoMapper;
using Domain.Exceptions.Auth;
using Domain.Exceptions.Booking;
using Domain.Models;

namespace Application.Services;

public class BookingService(IRepositoryManager repositoryManager, IEventService eventService, IMapper mapper, ICurrentUserService currentUser) : IBookingService
{
    private static readonly SemaphoreSlim _bookigSemaphore = new(1, 1);

    // Максимум активных броней на одного пользователя (пример)
    private const int MAX_ACTIVE_BOOKINGS_PER_USER = 20;

    public async Task<BookingDTO> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        if(!currentUser.IsAuthenticated)
            throw new WhoAreYouException($"{nameof(BookingService)}: Пользователь не авторизован");

        await _bookigSemaphore.WaitAsync(cancellationToken);

        try
        {
            // Проверяем, что событие не прошло (используем публичный метод сервиса событий)
            var eventInfo = await eventService.GetEventByIdAsync(eventId);
            if (eventInfo.StartAt <= DateTime.UtcNow)
                throw new BookingPastEventException(eventId);

            // Базовый подсчёт активных ожиданий (в текущей реализации считаем Pending)
            var pendingBookings = await repositoryManager.Booking.GetAllBookingsByUserIdAsync(currentUser.UserId);
            var userPendingCount = pendingBookings.Count(b => b.UserId == currentUser.UserId);
            if (userPendingCount >= MAX_ACTIVE_BOOKINGS_PER_USER)
                throw new BookingLimitExceededException(currentUser.UserId, MAX_ACTIVE_BOOKINGS_PER_USER);

            Booking booking = new Booking(eventId, currentUser.UserId);
            repositoryManager.Booking.CreateBooking(booking);

            await eventService.ReserveSeats((eventId: booking.EventId, seats: 1));

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

        await repositoryManager.SaveAsync();
    }

    public async Task RejectBooingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        var booking = await GetBookingAsync(bookingId);
        await eventService.ReleaseSeats((eventId: booking.EventId, seats: 1));
        booking.Reject();

        await repositoryManager.SaveAsync();
    }
}
