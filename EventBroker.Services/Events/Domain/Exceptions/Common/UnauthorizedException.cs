namespace Events.Domain.Exceptions.Common;

public abstract class UnauthorizedException(string message) : Exception(message)
{ }
