using Events.Domain.Exceptions.Common;

namespace Events.Domain.Exceptions;

public class AccessDeniedException : UnauthorizedException
{
    public AccessDeniedException(string message) :
        base($"Операция вам запрещена: {message}")
    { }
}
