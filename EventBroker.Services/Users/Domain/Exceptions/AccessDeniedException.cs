using Users.Domain.Exceptions.Common;

namespace Users.Domain.Exceptions;

public class AccessDeniedException : UnauthorizedException
{
    public AccessDeniedException(string message) :
        base($"Операция вам запрещена: {message}")
    { }
}
