namespace Bookings.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base($"Ошибка бизнес-логики: {message}")
    { }
}
