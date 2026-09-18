using AutoMapper;
using Bookings.Application.Common.DTO;
using Bookings.Application.Contracts.Messaging;
using Bookings.Application.Contracts.Persistence;
using Bookings.Application.Contracts.Services;
using Bookings.Domain.Exceptions;
using Bookings.Domain.Models;
using Enums.Users;
using Messaging;
using Messaging.Bookings;
using Microsoft.Extensions.Logging;
using System.ComponentModel.Design.Serialization;
using System.Threading;

namespace Bookings.Application.Services;

public class BookingService(IRepositoryManager repositoryManager, IOutboxService outboxService, IMapper mapper, ICurrentUserService currentUser, ILogger<BookingService> logger) : IBookingService
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
        if(stoppingToken.IsCancellationRequested)
        {
            logger.LogWarning("{TraceId} Подтверждение брони {BookingId} на событие {EventId}. Операция прервана", traceId, bookingId, eventId);
            return;
        }

        // Извлекаем бронь из репозитория
        var booking = await repositoryManager.Booking.GetByIdAsync(bookingId);

        // бронь на месте и ждет подтверждения
        if (booking is not null)
        {
            if(booking.Status != BookingStatus.Pending)
            {
                logger.LogWarning("{TraceId} Подтверждение брони {BookingId} на событие {EventId}]. Бронь уже в статусе {Status}", traceId, bookingId, eventId, booking.Status);
                return;
            }

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
            logger.LogWarning("{TraceId} Подтверждение брони {BookingId} на событие {EventId}. Бронь отсутсвует в БД", traceId, bookingId, eventId);
        }
    }

    public async Task RejectBooingAsync(Guid traceId, Guid bookingId, Guid eventId, Guid userId, string reason, CancellationToken stoppingToken = default)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            logger.LogWarning("{TraceId} Отклонение брони {BookingId} на событие {EventId}. Операция прервана", traceId, bookingId, eventId);
            return;
        }

        // Извлекаем бронь из репозитория
        var booking = await repositoryManager.Booking.GetByIdAsync(bookingId);

        // бронь на месте, проверка на возможность отклонить
        if (booking is not null)
        {
            if (booking.Status != BookingStatus.Pending)
            {
                logger.LogWarning("{TraceId} Отклонение брони {BookingId} на событие {EventId}]. Бронь уже в статусе {Status}", traceId, bookingId, eventId, booking.Status);
                return;
            }

            booking.Reject();

            if (booking is not null)
            {
                booking.Reject();

                // Формируем компенсирующее интеграционное событие для шины
                var cancelledEvent = new BookingRejected(
                    TraceId: traceId,
                    BookingId: booking?.Id ?? bookingId, // Страховка от null
                    EventId: booking?.EventId ?? eventId,
                    UserId: booking?.UserId ?? userId,
                    Reason: reason
                );
                await outboxService.EnqueueMessageAsync(cancelledEvent, Topics.BookingProcessing, stoppingToken);
            }
        }
        else
        {
            logger.LogWarning("{TraceId} Отклонение брони {BookingId} на событие {EventId}. Бронь отсутсвует в БД", traceId, bookingId, eventId);
        }
    }

    public async Task<bool> CancelBookingAsync(Guid bookingId, CancellationToken stoppingToken = default)
    {
        if (stoppingToken.IsCancellationRequested)
            throw new OperationCanceledException(stoppingToken);

        var booking = await repositoryManager.Booking.GetByIdAsync(bookingId) ??
            throw new BookingNotFoundException(bookingId);
       
        if (currentUser.Role == Role.Admin || currentUser.UserId == booking.UserId)
        {
            await outboxService.EnqueueMessageAsync(@event: new BookingCancelled(TraceId: Guid.CreateVersion7(),
                                                                                 BookingId: bookingId,
                                                                                 EventId: booking.EventId,
                                                                                 UserId: booking.UserId),
                                                    topic: Topics.BookingProcessing,
                                                    cancellationToken: stoppingToken);
            booking.Cancel();

            await repositoryManager.SaveAsync();
            return true;
        }
        else
            throw new AccessDeniedException("полностью отменить бронирование может только владелец брони или администратор сервиса");

    }

    public Task<bool> RemoveByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}