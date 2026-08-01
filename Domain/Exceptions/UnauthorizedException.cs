namespace Domain.Exceptions;

public abstract class UnauthorizedException(string message) : Exception(message)
{ }
