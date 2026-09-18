namespace Bookings.Application.Contracts.Messaging;

public interface IInboxService
{
    /// <summary> Быстрая проверка дубликатов на входе в конвеер </summary>
    Task<bool> HasBeenProcessedAsync(Guid messageId, string messageType, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Регистрация начала обработки.
    /// </summary>
    /// <param name="content"> outbox.context - используется только при ошибке, иначе всегда null</param>
    /// <returns>
    /// Возвращаем true - если сообщение готово к бизнес-обработке
    /// Возвращаем false - если дубликат, гонка экземпляров сервиса за БД или сообщение Poison Pill (битое, нечитаемое)
    /// </returns>
    Task<bool> ReceiveAsync(Guid messageId, string messageType, string content, CancellationToken cancellationToken = default);

    /// <summary> Фиксация успешного завершения бизнес-логики </summary>
    Task MarkAsProcessedAsync(Guid messageId, string messageType, CancellationToken cancellationToken = default);

    /// <summary> Фиксация ошибки для последующей  (redelivery - легальной повторной попытки после временного сбоя бизнеса </summary>
    Task MarkAsFailedAsync(Guid messageId, string messageType, string errorMessage, CancellationToken cancellationToken = default);
}
