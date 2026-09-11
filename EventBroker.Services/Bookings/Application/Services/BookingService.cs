using AutoMapper;
using Messaging;
using Messaging.Bookings;
using Bookings.Domain.Models;
using Bookings.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Bookings.Application.Common.DTO;
using Bookings.Application.Contracts.Services;
using Bookings.Application.Contracts.Messaging;
using Bookings.Application.Contracts.Persistence;

namespace Bookings.Application.Services;

public class BookingService(IRepositoryManager repositoryManager, IOutboxService outboxService, IMapper mapper, ICurrentUserService currentUser, ILogger logger) : IBookingService
{
    private static readonly SemaphoreSlim _bookigSemaphore = new(1, 1);

    // Максимум активных броней на одного пользователя (пример)
    private const int MAX_ACTIVE_BOOKINGS_PER_USER = 10;

    public async Task<BookingDTO> CreateBookingAsync(Guid eventId, CancellationToken stoppingToken = default)
    {
        if (stoppingToken.IsCancellationRequested)
            throw new OperationCanceledException(stoppingToken);

        if (!currentUser.IsAuthenticated)
            throw new WhoAreYouException($"{nameof(BookingService)}: Пользователь не авторизован");

        await _bookigSemaphore.WaitAsync(stoppingToken);

        try
        {
            var eventInfo = await repositoryManager.EventRead.GetByIdAsync(eventId);
            if (eventInfo!.StartAt <= DateTime.UtcNow)
                throw new BookingPastEventException(eventId);

            // Базовый подсчёт активных ожиданий (в текущей реализации считаем Pending)
            var pendingBookings = await repositoryManager.Booking.GetAllBookingsByUserIdAsync(currentUser.UserId);
            var userPendingCount = pendingBookings.Count(b => b.UserId == currentUser.UserId);
            if (userPendingCount >= MAX_ACTIVE_BOOKINGS_PER_USER)
                throw new BookingLimitExceededException(currentUser.UserId, MAX_ACTIVE_BOOKINGS_PER_USER);

            Booking booking = new Booking(eventId, currentUser.UserId);
            repositoryManager.Booking.CreateBooking(booking);

            await outboxService.EnqueueMessageAsync(@event: new BookingStarted(TraceId: Guid.CreateVersion7(),
                                                                               BookingId: booking.Id,
                                                                               EventId: booking.EventId,
                                                                               UserId: booking.UserId),
                                                    topic: Topics.BookingProcessing,
                                                    stoppingToken);

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
        var entity = await repositoryManager.Booking.GetByIdAsync(bookingId);
        if (entity is null)
            throw new BookingNotFoundException(bookingId);

        return mapper.Map<BookingDTO>(entity);
    }

    /// <summary> Consumer - метод саги бронирования метс на мероприятие </summary>
    /// <remarks> Вызов SaveChanges не допускается</remarks>
    public async Task ConfirmBookingAsync(
      Guid traceId,
      Guid bookingId,
      Guid eventId,
      Guid userId,
      CancellationToken stoppingToken = default)
    {
        // Извлекаем бронь из репозитория
        var booking = await repositoryManager.Booking.GetByIdAsync(bookingId);

        // 🟢 Счастливый путь (Happy Path): бронь на месте и ждет подтверждения
        if (booking is not null && booking.Status == BookingStatus.Pending)
        {
            // Вызываем доменный метод успеха
            booking.Confirm();

            var confirmedEvent = new BookingConfirmed(
                TraceId: traceId,
                BookingId: booking.Id,
                EventId: booking.EventId,
                UserId: booking.UserId
            );

            await outboxService.EnqueueMessageAsync(confirmedEvent, Topics.BookingProcessing, stoppingToken);
        }
        else
        {
            string cancelReason;

            if (booking is null)
            {
                cancelReason = "Бронь физически не существует в БД бронирований";
                logger.LogCritical("Авария данных саги: Получен ответ по несуществующей брони {BookingId}", bookingId);
            }
            else
            {
                cancelReason = $"Сервис событий отклонил запрос. Текущий статус брони в БД: {booking.Status}";
                booking.Reject();
            }

            // Формируем компенсирующее интеграционное событие для шины
            var cancelledEvent = new BookingCancelled(
                TraceId: traceId,
                BookingId: booking?.Id ?? bookingId, // Страховка от null
                EventId: booking?.EventId ?? eventId,
                UserId: booking?.UserId ?? userId,
                Reason: cancelReason
            );

            await outboxService.EnqueueMessageAsync(cancelledEvent, Topics.BookingProcessing, stoppingToken);
        }
    }

    public Task RejectBooingAsync(Guid traceId, Guid bookingId, Guid eventId, Guid userId, CancellationToken stoppingToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CancelBookingAsync(Guid traceId, Guid bookingId, Guid userId, CancellationToken stoppingToken = default)
    {
        throw new NotImplementedException();
    }
}
