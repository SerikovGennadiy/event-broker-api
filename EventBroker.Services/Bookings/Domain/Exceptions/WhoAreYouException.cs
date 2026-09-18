namespace Bookings.Domain.Exceptions;

/// <summary> Исключение для генерации сообщения 401 (не авторизован) </summary>
public class WhoAreYouException(string message) : Exception(message)
{ }
