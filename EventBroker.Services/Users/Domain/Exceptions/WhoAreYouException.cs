namespace Domain.Exceptions.Auth;

/// <summary> Исключение для генерации сообщения 401 (не авторизован) </summary>
public class WhoAreYouException(string message) : Exception(message)
{ }
